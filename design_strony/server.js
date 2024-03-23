const express = require('express');
const axios = require('axios');
const https = require('https');
const sql = require('mssql');
const cron = require('node-cron');

const app = express();
const port = 3000;

app.use(express.static('client'));
app.use(express.json());

const config = {
    server: 'localhost',
    database: 'Nokaut',
    port: 1433,
    user: 'NodeServer',
    options: {
        trustedConnection: true,
        encrypt: false,
    }
};


process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'; 

const apiUrl = 'https://localhost:44345/GetProduct/products/';

async function establishConnection() {
    try {
        const pool = await sql.connect(dbConfig);
        console.log('Connected to MSSQL database.');
        return pool;
    } catch (error) {
        console.error('Error establishing connection to MSSQL database:', error);
        throw error;
    }
}


//Insert new into database
async function saveToDatabase(Name, URL, Price, ImageURL, TypeProduct) {
  try {
    const request = pool.request();

    request.input('Name', sql.NVarChar, Name);
    request.input('URL', sql.NVarChar, URL);
    request.input('Price', sql.Decimal(18,2), Price);
    request.input('ImageURL', sql.NVarChar, ImageURL);
    request.input('TypeProduct', sql.NVarChar, TypeProduct);

    const query = `INSERT INTO Products (Name, URL, Price, ImageURL, TypeProduct) 
                   VALUES (@Name, @URL, @Price, @ImageURL, @TypeProduct)`;

    await request.query(query);
    //console.log('Data saved to MSSQL database.');
  } catch (error) {
    console.error('Error saving data to MSSQL database:', error);
  }
}

//Delete old from database
async function deleteFromDatabase(TypeProduct) {
    const request = pool.request();
    const deleteQuery = `DELETE FROM Products WHERE TypeProduct = @TypeProduct`;
    request.input('TypeProduct', sql.NVarChar, TypeProduct);
    await request.query(deleteQuery);
}

//HTML invoke scraping
app.post('/productType', async (req, res) => {
    const { productType } = req.body;
    try {
        const request = pool.request();
        request.input('TypeProduct', sql.NVarChar, productType);
        const checkQuery = `SELECT * FROM Products WHERE TypeProduct = @TypeProduct`;
        const result = await request.query(checkQuery);

        if (result.recordset.length > 0) {
            console.log(`Products of type ${productType} already exist in the database.`);
            res.status(200).json(result.recordset);

        } else {
            console.log(`Products of type ${productType} not found in the database. Fetching from API.`);
            const response = await axios.get(`${apiUrl}${productType}`);

            response.data.forEach(async product => {
                const { name, url, price, imageUrl, typeProduct } = product;
                //console.log(`Extracted values:`, { name, url, imageUrl, price, typeProduct });
                const numericPrice = price.value;
                await saveToDatabase(name, url, numericPrice, imageUrl, typeProduct);
            });

            const checkQuery = `SELECT * FROM Products WHERE TypeProduct = @TypeProduct`;
            const result = await request.query(checkQuery);
            res.status(200).json(result.recordset);
        }
    } catch (error) {
        console.error('Error:', error);
        res.status(500).send('Internal Server Error.');
    }
});


//Looking for all of the productType in database
async function findAllProductTypes() {
    try {
        const request = pool.request();
        const query = `SELECT DISTINCT TypeProduct FROM Products`;
        const result = await request.query(query);
        return result.recordset.map(row => row.TypeProduct);
    } catch (error) {
        console.error('Error fetching product types from the database:', error);
        throw error;
    }
}

//Scheduled scraping
async function scrapeProductsForType(productType) {
    try {
        const response = await axios.get(`${apiUrl}${productType}`);
        response.data.forEach(async product => {
            const { name, url, imageUrl, price, typeProduct } = product;
            const numericPrice = price.value;
            await deleteFromDatabase(typeProduct);
            await saveToDatabase(name, url, numericPrice, imageUrl, typeProduct);
        });
        console.log(`Scraping completed for product type: ${productType}`);
    } catch (error) {
        console.error(`Error scraping products for product type ${productType}:`, error);
    }
}

//Schedule scraping
async function scheduleScraping() {
    try {
        await establishConnection();

        cron.schedule('*/1 * * * *', async () => {
            try {
                const productTypes = await findAllProductTypes();
                productTypes.forEach(productType => {
                scrapeProductsForType(productType);
                console.log(`Scraping scheduled for product type: ${productType}`);

            });
            }catch(error) {
            console.error(error);
            }
        });
    } catch (error) {
        console.error('Error scheduling scraping:', error);
    }
}

scheduleScraping();
await establishConnection();

app.listen(port, () => {
    console.log(`Server is listening at http://localhost:${port}`);
});


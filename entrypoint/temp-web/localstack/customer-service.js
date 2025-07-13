// LocalStack Backend Service for DMS
// This would be a Lambda Function in AWS/LocalStack

// Customer Schema
/*
{
  id: string/number,
  name: string,
  phone: string,
  email: string,
  type: string,
  lastContact: string (date)
}
*/

// DynamoDB Table: 'dms_customers'

// Example Lambda Function code (Node.js)
const AWS = require('aws-sdk');

// Configure AWS SDK for LocalStack
const dynamoDB = new AWS.DynamoDB.DocumentClient({
    endpoint: 'http://localhost:4566', // LocalStack default endpoint
    region: 'us-east-1',
    accessKeyId: 'test',
    secretAccessKey: 'test'
});

// Table name
const CUSTOMERS_TABLE = 'dms_customers';

exports.handler = async (event) => {
    console.log('Event received:', JSON.stringify(event));
    
    // Get the HTTP method and path
    const httpMethod = event.httpMethod;
    const path = event.path;
    
    try {
        // Simple health check endpoint
        if (path === '/health' && httpMethod === 'GET') {
            return {
                statusCode: 200,
                headers: corsHeaders,
                body: JSON.stringify({ status: 'ok', message: 'LocalStack is running' })
            };
        }
        // Route the request based on method and path
        else if (path === '/dms/api/init' && httpMethod === 'POST') {
            // Initialize database
            await createTableIfNotExists();
            return {
                statusCode: 200,
                headers: corsHeaders,
                body: JSON.stringify({ success: true, message: 'Database initialized' })
            };
        } 
        else if (path === '/dms/api/customers' && httpMethod === 'GET') {
            // Get all customers
            const customers = await getAllCustomers();
            return {
                statusCode: 200,
                headers: corsHeaders,
                body: JSON.stringify({ success: true, data: customers })
            };
        }
        else if (path === '/dms/api/customers' && httpMethod === 'POST') {
            // Add a new customer
            const customer = JSON.parse(event.body);
            const result = await addCustomer(customer);
            return {
                statusCode: 201,
                headers: corsHeaders,
                body: JSON.stringify({ success: true, data: result })
            };
        }
        else if (path.startsWith('/dms/api/customers/') && httpMethod === 'PUT') {
            // Update a customer
            const id = parseInt(path.split('/').pop());
            const updates = JSON.parse(event.body);
            const result = await updateCustomer(id, updates);
            return {
                statusCode: 200,
                headers: corsHeaders,
                body: JSON.stringify({ success: true, data: result })
            };
        }
        else if (path.startsWith('/dms/api/customers/') && httpMethod === 'DELETE') {
            // Delete a customer
            const id = parseInt(path.split('/').pop());
            await deleteCustomer(id);
            return {
                statusCode: 200,
                headers: corsHeaders,
                body: JSON.stringify({ success: true, message: 'Customer deleted' })
            };
        }
        else {
            // Route not found
            return {
                statusCode: 404,
                headers: corsHeaders,
                body: JSON.stringify({ success: false, message: 'Route not found' })
            };
        }
    } catch (error) {
        console.error('Error processing request:', error);
        return {
            statusCode: 500,
            headers: corsHeaders,
            body: JSON.stringify({ success: false, message: error.message })
        };
    }
};

// CORS headers for API responses
const corsHeaders = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Headers': 'Content-Type,Authorization',
    'Access-Control-Allow-Methods': 'OPTIONS,GET,PUT,POST,DELETE'
};

// Database Operations
async function createTableIfNotExists() {
    const dynamoDB_raw = new AWS.DynamoDB({
        endpoint: 'http://localhost:4566',
        region: 'us-east-1',
        accessKeyId: 'test',
        secretAccessKey: 'test'
    });
    
    try {
        await dynamoDB_raw.describeTable({ TableName: CUSTOMERS_TABLE }).promise();
        console.log(`Table ${CUSTOMERS_TABLE} already exists`);
        return true;
    } catch (error) {
        if (error.code === 'ResourceNotFoundException') {
            // Table doesn't exist, create it
            const params = {
                TableName: CUSTOMERS_TABLE,
                KeySchema: [{ AttributeName: 'id', KeyType: 'HASH' }],
                AttributeDefinitions: [{ AttributeName: 'id', AttributeType: 'N' }],
                ProvisionedThroughput: {
                    ReadCapacityUnits: 5,
                    WriteCapacityUnits: 5
                }
            };
            
            await dynamoDB_raw.createTable(params).promise();
            console.log(`Table ${CUSTOMERS_TABLE} created`);
            return true;
        } else {
            throw error;
        }
    }
}

async function getAllCustomers() {
    const params = {
        TableName: CUSTOMERS_TABLE
    };
    
    const result = await dynamoDB.scan(params).promise();
    return result.Items || [];
}

async function addCustomer(customer) {
    // If no ID is provided, generate one
    if (!customer.id) {
        // Get highest current ID
        const allCustomers = await getAllCustomers();
        const maxId = allCustomers.length > 0 
            ? Math.max(...allCustomers.map(c => Number(c.id)))
            : 0;
        customer.id = maxId + 1;
    }
    
    const params = {
        TableName: CUSTOMERS_TABLE,
        Item: {
            ...customer,
            id: Number(customer.id) // Ensure ID is a number
        }
    };
    
    await dynamoDB.put(params).promise();
    return params.Item;
}

async function updateCustomer(id, updates) {
    // First get the existing customer
    const getParams = {
        TableName: CUSTOMERS_TABLE,
        Key: { id: Number(id) }
    };
    
    const { Item } = await dynamoDB.get(getParams).promise();
    
    if (!Item) {
        throw new Error(`Customer with ID ${id} not found`);
    }
    
    // Merge existing data with updates
    const updatedCustomer = {
        ...Item,
        ...updates,
        id: Number(id) // Ensure ID remains unchanged and is a number
    };
    
    const putParams = {
        TableName: CUSTOMERS_TABLE,
        Item: updatedCustomer
    };
    
    await dynamoDB.put(putParams).promise();
    return updatedCustomer;
}

async function deleteCustomer(id) {
    const params = {
        TableName: CUSTOMERS_TABLE,
        Key: { id: Number(id) }
    };
    
    await dynamoDB.delete(params).promise();
    return true;
}

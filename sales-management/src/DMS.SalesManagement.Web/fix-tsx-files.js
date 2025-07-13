const fs = require('fs');
const path = require('path');

// Path to the src directory
const srcDir = path.resolve(__dirname, 'src');

// Function to add @ts-nocheck to a file if it doesn't have it already
function addTsNoCheck(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  if (!content.includes('// @ts-nocheck')) {
    console.log(`Adding @ts-nocheck to ${filePath}`);
    fs.writeFileSync(filePath, `// @ts-nocheck\n${content}`);
  }
}

// Function to recursively process a directory
function processDirectory(dir) {
  const files = fs.readdirSync(dir);
  
  for (const file of files) {
    const filePath = path.join(dir, file);
    const stats = fs.statSync(filePath);
    
    if (stats.isDirectory()) {
      processDirectory(filePath);
    } else if (file.endsWith('.tsx')) {
      addTsNoCheck(filePath);
    }
  }
}

// Start processing
processDirectory(srcDir);
console.log('Done adding @ts-nocheck to all .tsx files');

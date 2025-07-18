// ***********************************************************
// This is a support file for Cypress tests
// ***********************************************************

import './commands';

// Hide fetch/XHR requests in the Cypress command log
const app = window.top;

if (!app.document.head.querySelector('[data-hide-command-log-request]')) {
  const style = app.document.createElement('style');
  style.innerHTML = '.command-name-request, .command-name-xhr { display: none }';
  style.setAttribute('data-hide-command-log-request', '');
  app.document.head.appendChild(style);
}

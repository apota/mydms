using DMS.FinancialManagement.Core.DTOs;
using DMS.FinancialManagement.Core.Models;
using DMS.FinancialManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DMS.FinancialManagement.API.Controllers
{
    [ApiController]
    [Route("api/finance/accounts")]
    [Authorize]
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly IChartOfAccountService _accountService;
        private readonly ILogger<ChartOfAccountsController> _logger;

        public ChartOfAccountsController(IChartOfAccountService accountService, ILogger<ChartOfAccountsController> logger)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all accounts", Description = "Retrieves a paginated list of all accounts in the chart of accounts")]
        [SwaggerResponse(200, "List of accounts retrieved successfully", typeof(IEnumerable<ChartOfAccountDto>))]
        public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetAllAccounts(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var accounts = await _accountService.GetAllAccountsAsync(skip, take);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving accounts");
            }
        }

        [HttpGet("hierarchy")]
        [SwaggerOperation(Summary = "Get account hierarchy", Description = "Retrieves the full hierarchical structure of the chart of accounts")]
        [SwaggerResponse(200, "Account hierarchy retrieved successfully", typeof(IEnumerable<ChartOfAccountDto>))]
        public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetAccountHierarchy(
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var accountHierarchy = await _accountService.GetAccountHierarchyAsync(includeInactive);
                return Ok(accountHierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account hierarchy");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving account hierarchy");
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get account by ID", Description = "Retrieves a specific account by its ID")]
        [SwaggerResponse(200, "Account retrieved successfully", typeof(ChartOfAccountDto))]
        [SwaggerResponse(404, "Account not found")]
        public async Task<ActionResult<ChartOfAccountDto>> GetAccountById(Guid id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound($"Account with ID {id} not found");
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account with ID: {AccountId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving account");
            }
        }

        [HttpGet("type/{accountType}")]
        [SwaggerOperation(Summary = "Get accounts by type", Description = "Retrieves accounts of a specific type")]
        [SwaggerResponse(200, "Accounts retrieved successfully", typeof(IEnumerable<ChartOfAccountDto>))]
        public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetAccountsByType(AccountType accountType)
        {
            try
            {
                var accounts = await _accountService.GetAccountsByTypeAsync(accountType);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts of type: {AccountType}", accountType);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving accounts");
            }
        }

        [HttpGet("parent/{parentId:guid}")]
        [SwaggerOperation(Summary = "Get accounts by parent ID", Description = "Retrieves child accounts for a specific parent account")]
        [SwaggerResponse(200, "Child accounts retrieved successfully", typeof(IEnumerable<ChartOfAccountDto>))]
        public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetAccountsByParentId(Guid parentId)
        {
            try
            {
                var accounts = await _accountService.GetAccountsByParentIdAsync(parentId);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving child accounts for parent ID: {ParentId}", parentId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving accounts");
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create account", Description = "Creates a new account in the chart of accounts")]
        [SwaggerResponse(201, "Account created successfully", typeof(ChartOfAccountDto))]
        [SwaggerResponse(400, "Invalid account data")]
        public async Task<ActionResult<ChartOfAccountDto>> CreateAccount([FromBody] ChartOfAccountCreateDto accountDto)
        {
            try
            {
                if (accountDto == null)
                {
                    return BadRequest("Account data is null");
                }

                var createdAccount = await _accountService.CreateAccountAsync(accountDto);
                return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.Id }, createdAccount);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating account");
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update account", Description = "Updates an existing account")]
        [SwaggerResponse(200, "Account updated successfully", typeof(ChartOfAccountDto))]
        [SwaggerResponse(404, "Account not found")]
        [SwaggerResponse(400, "Invalid account data")]
        public async Task<ActionResult<ChartOfAccountDto>> UpdateAccount(Guid id, [FromBody] ChartOfAccountUpdateDto accountDto)
        {
            try
            {
                if (accountDto == null)
                {
                    return BadRequest("Account data is null");
                }

                var updatedAccount = await _accountService.UpdateAccountAsync(id, accountDto);
                if (updatedAccount == null)
                {
                    return NotFound($"Account with ID {id} not found");
                }

                return Ok(updatedAccount);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account with ID: {AccountId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating account");
            }
        }

        [HttpPut("{id:guid}/deactivate")]
        [SwaggerOperation(Summary = "Deactivate account", Description = "Deactivates an existing account")]
        [SwaggerResponse(200, "Account deactivated successfully")]
        [SwaggerResponse(404, "Account not found")]
        public async Task<ActionResult> DeactivateAccount(Guid id)
        {
            try
            {
                var result = await _accountService.DeactivateAccountAsync(id);
                if (!result)
                {
                    return NotFound($"Account with ID {id} not found");
                }

                return Ok(new { Message = "Account deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating account with ID: {AccountId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deactivating account");
            }
        }

        [HttpPut("{id:guid}/activate")]
        [SwaggerOperation(Summary = "Activate account", Description = "Activates an existing account")]
        [SwaggerResponse(200, "Account activated successfully")]
        [SwaggerResponse(404, "Account not found")]
        public async Task<ActionResult> ActivateAccount(Guid id)
        {
            try
            {
                var result = await _accountService.ActivateAccountAsync(id);
                if (!result)
                {
                    return NotFound($"Account with ID {id} not found");
                }

                return Ok(new { Message = "Account activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating account with ID: {AccountId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error activating account");
            }
        }
    }
}

/*
 * REFACTORED FOR PART 3
 * This controller now uses the FunctionApiClient to manage files,
 * meeting the "must use functions" requirement.
 */
using AzureRetailHub.Models;
using AzureRetailHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace AzureRetailHub.Controllers
{
    public class ContractsController : Controller
    {
        private readonly FunctionApiClient _api;

        // We only inject the FunctionApiClient
        public ContractsController(FunctionApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            // Call the function to get the list of files
            var fileList = await _api.GetContractsAsync();
            return View(fileList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile contractFile)
        {
            if (contractFile == null || contractFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please choose a file to upload.";
                return RedirectToAction(nameof(Index));
            }

            var fileName = Path.GetFileName(contractFile.FileName);
            await using var ms = new MemoryStream();
            await contractFile.CopyToAsync(ms);

            // Call the function to upload the file
            var success = await _api.UploadContractAsync(ms, fileName);

            if (success)
            {
                TempData["SuccessMessage"] = "File uploaded successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "File upload failed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Note: Download and Delete actions are not explicitly required
        // by the POE and can be removed for simplicity if you wish.
        // We will leave them out for now to focus on core requirements.
    }
}
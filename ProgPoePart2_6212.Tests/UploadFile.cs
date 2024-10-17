
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ProgPoePart2_6212.Models;
using Microsoft.AspNetCore.Http;


public class FileUpload
{
    private List<LecturerClaim> _claims;

    public FileUpload(List<LecturerClaim> claims)
    {
        _claims = claims;
    }

    public async Task<string> UploadDocument(int claimId, IFormFile file)
    {
        var claim = _claims.Find(c => c.Id == claimId);

        if (claim == null)
        {
            return "ClaimNotFound";
        }

        if (file == null || file.Length == 0)
        {
            return "InvalidFile";
        }

        // Simulate file processing (e.g., saving to a server or database)
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (fileExtension != ".pdf")
        {
            return "UnsupportedFileType";
        }

        // Simulate adding the file to the claim's documents
        var Suppdocument = new SuppDocument
        {
            FileName = file.FileName,

            Id = claimId
        };

        claim.Documents.Add(Suppdocument); // Assume `Documents` is a list of files for the claim

        return "FileUploaded";
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lpr.Api.Controllers
{
    public class LprController : Controller
    {
        [HttpPost, Consumes("multipart/form-data")]
        public async Task<string> Recognize()
        {
            List<IFormFile> files = Request.Form.Files.ToList();
            long size = files.Sum(f => f.Length);

            var filePath = Path.GetTempPath();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath + formFile.FileName, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    var psi = new ProcessStartInfo
                    {
                        FileName = "alpr",
                        Arguments = $"-c in {filePath}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var p = new Process();
                    p.StartInfo = psi;
                    var output = "";
                    var error = "";
                    p.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        output += e.Data + Environment.NewLine;
                    };
                    p.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        error += e.Data + Environment.NewLine;
                    };
                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                    if (error.Replace(Environment.NewLine, "").Replace(" ", "") != "")
                    {
                        return error;
                    }
                    return output;
                }
            }
            return "Not Found";
        }
    }
}
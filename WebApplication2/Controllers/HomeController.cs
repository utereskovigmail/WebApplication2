using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication2.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;



using WebApplication2;



public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Upload(IFormFile photo, string forbiddenWords, bool isFile)
    {
        List<string> words = new List<string>();
        if (isFile)
        {
            if (photo == null || photo.Length == 0)
            {
                Console.WriteLine("We are here");
                ModelState.AddModelError("photo", "Empty file");
                
                return View("Index");
            }
            
            using var stream = new MemoryStream();
            photo.CopyTo(stream);
            var content = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            words = content.Split(" ").ToList();
            stream.Close();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(forbiddenWords))
            {
                ModelState.AddModelError("forbiddenWords", "Empty string");
                return View("Index");
            }
            words = forbiddenWords.Split(" ").ToList();
        }
        
        
        string filePath = "/Users/utereskovygmail.com/Desktop"; 
        Search(filePath, words);
        
        Console.WriteLine("Finished");
        

        return View("Index");
    }

    public IActionResult Privacy()
    {
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    
    
    
    
    
    static void Search(string filePath, List<string> words)
    {
        string name = Path.Combine(filePath, "BackUpFolder");
        string fname = Path.Combine(name, "Log") + ".txt";
        Directory.CreateDirectory(name);
        if (!System.IO.File.Exists(fname))
        {
            using (FileStream fs = System.IO.File.Create(fname))
            {
            }
        }
        else
        {
            System.IO.File.WriteAllText(fname, string.Empty);
        }


        string root = "/Users/utereskovygmail.com";
        
        Recursion(root, words,name, fname);
        
    }
    
    
    
    
    static void Recursion(string path, List<string> words, string parentPath, string log)
    {
        const int MAX_PATH_LENGTH = 220; 

        if (path.Length > MAX_PATH_LENGTH || 
            path.Contains("_refs/revlinks") || 
            path.Contains("BackUpFolder")||
            path.Contains("/Library")||
            path.Contains("/.Trash") ||   
            path.Contains("/.DS_Store"))
            return;


        
        
        
        
        
        
        string[] files;
        try
        {
            files = Directory.GetFiles(path);
        }
        catch (UnauthorizedAccessException)
        {
            return; 
        }
        
        
        foreach (string file in files)
        {
            if (file.EndsWith(".txt"))
            {
                try
                {

                    string line = System.IO.File.ReadAllText(file);
                    bool found = false;

                    foreach (string word in words)
                    {
                        if (line.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            string destination = Path.Combine(parentPath, Path.GetFileNameWithoutExtension(file));
                            Directory.CreateDirectory(destination);
                            string dest1 = Path.Combine(destination, Path.GetFileName(file));
                            System.IO.File.Copy(file, dest1, true);
                            found = true;
                            string textToAppend = "\n";
                            textToAppend += file;
                            textToAppend += "\t";
                            FileInfo fileInfo = new FileInfo(file);
                            long fileSizeInBytes = fileInfo.Length;
                            textToAppend += fileSizeInBytes.ToString() + " - size in bytes\t";
                                
                            System.IO.File.AppendAllText(log, Environment.NewLine + textToAppend);
                            
                            
                            break;
                        }
                    }

                    if (found)
                    {
                        int count = 0;
                        foreach (string word in words)
                        {
                            if (line.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                count += CountOccurrences(line, word);
                                line = Regex.Replace(line, Regex.Escape(word), "*******", RegexOptions.IgnoreCase);
                            }
                        }
                        
                        string textToAppend = "";
                        textToAppend += count + " - number of changes";
                        System.IO.File.AppendAllText(log, Environment.NewLine + textToAppend);
                        

                        string destination = parentPath + "/" + Path.GetFileNameWithoutExtension(file) +"/" + Path.GetFileNameWithoutExtension(file) + "Revised.txt";
                        System.IO.File.CreateText(destination).Close();
                        System.IO.File.WriteAllText(destination, line);
                    }
                }
                catch
                {
                    continue;
                }
                
            }
        }
        
        string[] dirs;
        try
        {
            dirs = Directory.GetDirectories(path);
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        Parallel.ForEach(dirs, dir =>
        {
            Recursion(dir, words, parentPath, log);
        });
    }
    
    static int CountOccurrences(string text, string word)
    {
        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += word.Length;
        }

        return count;
    }

}


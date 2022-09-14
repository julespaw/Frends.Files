using Frends.Files.LocalBackup.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Frends.Files.LocalBackup.Tests;

[TestClass]
public class UnitTests
{
    private readonly string _dir = Path.Combine(Environment.CurrentDirectory, "Tests"); // ...Test\bin\Debug\net6.0\
    Input? input;

    [TestInitialize]
    public void Setup()
    {
        CreateTestFiles();
    }

    [TestCleanup]
    public void TearDown()
    {
        DeleteTestFolder();
    }

    /// <summary>
    /// Copy all files to backup directory. Don't create subdir.
    /// </summary>
    [TestMethod]
    public void CopyAll_CreateSubdirectoriesFalse_Test()
    {
        var buDir = Path.Combine(_dir, "Backup");

        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "*",
            BackupDirectory = buDir,
            TaskExecutionId = null,
            DaysOlder = 5,
            Cleanup = false,
            CreateSubdirectories = false
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);

        foreach (var x in Directory.GetDirectories(buDir))
        {
            string[] files = Directory.GetFiles(x);
            foreach (string file in files)
            {
                Assert.IsTrue(file.Contains($@"{x}\Overwrite.txt") || file.Contains($@"{x}\Test1.txt") || file.Contains($@"{x}\Test2.txt") || file.Contains($@"{x}\Test1.xml"));  
            }
        }
    }

    /// <summary>
    /// Copy all files to backup directory. Create subdir.
    /// </summary>
    [TestMethod]
    public void CopyAll_CreateSubdirectoriesTrue_Test()
    {
        var buDir = Path.Combine(_dir, "Backup");

        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "*",
            BackupDirectory = buDir,
            TaskExecutionId = Guid.NewGuid().ToString(),
            DaysOlder = 5,
            Cleanup = false,
            CreateSubdirectories = true
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);

        foreach (var x in Directory.GetDirectories(buDir, "2022-05-*"))
        {
            string[] files = Directory.GetFiles(x);
            foreach (string file in files)
            {
                Assert.IsTrue(file.Contains($@"{x}\Overwrite.txt") || file.Contains($@"{x}\Test1.txt") || file.Contains($@"{x}\Test2.txt") || file.Contains($@"{x}\Test1.xml"));
            }
        }
    }

    /// <summary>
    /// Copy all files to backup directory. Create subdir and use something else but GUID as TaskExecutionId.
    /// </summary>
    [TestMethod]
    public void CopyAll_CreateSubdirectoriesTrue_NonGUID_Test()
    {
        var buDir = Path.Combine(_dir, "Backup");

        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "*",
            BackupDirectory = buDir,
            TaskExecutionId = "qwerty123",
            DaysOlder = 5,
            Cleanup = false,
            CreateSubdirectories = true
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);

        foreach (var x in Directory.GetDirectories(buDir, "*qwerty123*"))
        {
            string[] files = Directory.GetFiles(x);
            foreach (string file in files)
            {
                Assert.IsTrue(file.Contains($@"{x}\Overwrite.txt") || file.Contains($@"{x}\Test1.txt") || file.Contains($@"{x}\Test2.txt") || file.Contains($@"{x}\Test1.xml"));
            }
        }
    }

    /// <summary>
    /// Copy only Test1.txt and Test2.txt.
    /// </summary>
    [TestMethod]
    public void CopyWithPrefixTest()
    {
        var buDir = Path.Combine(_dir, "Backup");

        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "Test*",
            BackupDirectory = buDir,
            TaskExecutionId = Guid.NewGuid().ToString(),
            DaysOlder = 5,
            Cleanup = false,
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);

        foreach (var x in Directory.GetDirectories(buDir, "2022-05-*"))
        {
            string[] files = Directory.GetFiles(x);
            foreach (string file in files)
            {
                Assert.IsTrue(file.Contains($@"{x}\Test1.txt") || file.Contains($@"{x}\Test2.txt") || file.Contains($@"{x}\Test1.xml"));
                Assert.IsTrue(!file.Contains($@"{x}\Overwrite.txt"));
            }
        }
    }

    /// <summary>
    /// Copy only Test1.txt and Test1.xml.
    /// </summary>
    [TestMethod]
    public void CopyWithPrefix2Test()
    {
        var buDir = Path.Combine(_dir, "Backup");

        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "Test1.(txt|xml)",
            BackupDirectory = buDir,
            TaskExecutionId = Guid.NewGuid().ToString(),
            DaysOlder = 5,
            Cleanup = false,
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);

        foreach (var x in Directory.GetDirectories(buDir, "2022-05-*"))
        {
            string[] files = Directory.GetFiles(x);
            foreach (string file in files)
            {
                Assert.IsTrue(file.Contains($@"{x}\Test1.txt") || file.Contains($@"{x}\Test1.xml"));
                Assert.IsTrue(!file.Contains($@"{x}\Overwrite.txt") || !file.Contains($@"{x}\Test2.txt"));
            }
        }
    }

    /// <summary>
    /// Copy only Test1.xml.
    /// </summary>
    [TestMethod]
    public void CopyWithPrefix3Test()
    {
        var buDir = Path.Combine(_dir, "Backup");

        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "Test1.[^t][^x][^t]",
            BackupDirectory = buDir,
            TaskExecutionId = Guid.NewGuid().ToString(),
            DaysOlder = 5,
            Cleanup = false,
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);

        foreach (var x in Directory.GetDirectories(buDir, "2022-05-*"))
        {
            string[] files = Directory.GetFiles(x);
            foreach (string file in files)
            {
                Assert.IsTrue(file.Contains($@"{x}\Test1.xml"));
                Assert.IsTrue(!file.Contains($@"{x}\Test1.txt") || !file.Contains($@"{x}\Overwrite.txt") || !file.Contains($@"{x}\Test2.txt"));
            }
        }
    }

    /// <summary>
    /// Copy pro_test.txt, pref_test.txt, _test.txt and skip prof_test.txt, pro_tet.txt.
    /// </summary>
    [TestMethod]
    public void CopyWithPrefix4Test()
    {
        var buDir = Path.Combine(_dir, "Backup\\Pro");

        input = new Input()
        {
            SourceDirectory = Path.Combine(_dir, "Pro"),
            SourceFile = "<regex>^(?!prof).*_test.txt",
            BackupDirectory = buDir,
            TaskExecutionId = Guid.NewGuid().ToString(),
            DaysOlder = 5,
            Cleanup = false,
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);

        foreach (var x in Directory.GetDirectories(buDir, "2022-05-*"))
        {
            string[] files = Directory.GetFiles(x);
            foreach (string file in files)
            {
                Assert.IsTrue(file.Contains($@"{x}\pro_test.txt") || file.Contains($@"{x}\pref_test.txt") || file.Contains($@"{x}\_test.txt"));
                Assert.IsTrue(!file.Contains($@"{x}\prof_test.txt") || !file.Contains($@"{x}\pro_tet.txt"));
            }
        }
    }

    /// <summary>
    /// Delete files older than {DaysOlder}. CreateSubdirectories = true.
    /// </summary>
    [TestMethod]
    public void CleanupFile_CreateSubdirectoriesTrue_Test()
    {
        var timestampString = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd_HH_mm_ss");
        var backupDirectory = Path.Combine($@"{_dir}\Cleanup\", $"{timestampString}-{Guid.NewGuid()}");
        Directory.CreateDirectory(backupDirectory);

        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "*",
            BackupDirectory = $@"{_dir}\Cleanup",
            TaskExecutionId = Guid.NewGuid().ToString(),
            DaysOlder = 1,
            Cleanup = true,
            CreateSubdirectories = true,
        };

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Delete files older than {DaysOlder}. CreateSubdirectories = false.
    /// </summary>
    [TestMethod]
    public void CleanupFile_CreateSubdirectoriesFalse_Test()
    {
        input = new Input()
        {
            SourceDirectory = _dir,
            SourceFile = "*",
            BackupDirectory = $@"{_dir}\Cleanup",
            TaskExecutionId = null,
            DaysOlder = 1,
            Cleanup = true,
            CreateSubdirectories = false,
        };
        
        
        var backupDirectory = Path.Combine($@"{_dir}\Cleanup\", $"DeleteThis");
        Directory.CreateDirectory(backupDirectory);
        Directory.SetLastWriteTimeUtc(backupDirectory, DateTime.Now.AddDays(-2));

        var result = Files.LocalBackup(input, default);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Cleanups);
        Assert.IsFalse(Directory.Exists(backupDirectory));
    }

    public void CreateTestFiles()
    {
        Directory.CreateDirectory($@"{_dir}\Sub");
        Directory.CreateDirectory($@"{_dir}\Pro");

        var list = new List<string>
        {
            $@"{_dir}\Test1.txt",
            $@"{_dir}\Test2.txt",
            $@"{_dir}\Test1.xml",
            $@"{_dir}\Overwrite.txt",
            $@"{_dir}\Sub\Overwrite.txt",
            $@"{_dir}\Pro\pro_test.txt",
            $@"{_dir}\Pro\pref_test.txt",
            $@"{_dir}\Pro\_test.txt",
            $@"{_dir}\Pro\prof_test.txt",
            $@"{_dir}\Pro\pro_tet.txt",
        };

        //create test files and edit creationdate
        foreach (var file in list)
        {
            if (file.StartsWith($@"{_dir}\Overwrite.txt"))
                File.AppendAllText(file, $"Overwrite complete.");
            else
                File.AppendAllText(file, $"Test {file}");
        }
    }

    public void DeleteTestFolder()
    {
        DirectoryInfo directoryInfo = new(_dir);
        directoryInfo.Delete(true);
    }
}

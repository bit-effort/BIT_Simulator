using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIT_Simulator.FileSystem
{
    [MoonSharpUserData]
    internal class BitFileSystem
    {
        public bool file_exists(string path)
        {
            return File.Exists(path);
        }

        public string read_file(string path)
        {
            return File.ReadAllText(path);
        }

        public void write_file(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public void append_file(string path, string content)
        {
            File.AppendAllText(path, content);
        }

        public void delete_file(string path)
        {
            File.Delete(path);
        }

        public string[] list_directory(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFileSystemEntries(path);
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        public string get_dir_name(string path)
        {
            return Path.GetFileName(path.TrimEnd('/', '\\')) ?? "";
        }
    }
}

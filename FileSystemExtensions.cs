using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FireAnt.IO
{
    public static class FileSystemExtensions
    {
        public static IEnumerable<DirectoryInfo> EnumerateDirectories ( this DirectoryInfo target )
        {
            return EnumerateDirectories ( target, "*" );
        }

        public static IEnumerable<DirectoryInfo> EnumerateDirectories ( this DirectoryInfo target, string searchPattern )
        {
            string searchPath = Path.Combine ( target.FullName, searchPattern );
            NativeWin32.WIN32_FIND_DATA findData;
            using (NativeWin32.SafeSearchHandle hFindFile = NativeWin32.FindFirstFile ( searchPath, out findData ))
            {
                if ( !hFindFile.IsInvalid )
                {
                    do
                    {
                        if ( ( findData.dwFileAttributes & FileAttributes.Directory ) != 0 && findData.cFileName != "." && findData.cFileName != ".." )
                        {
                            yield return new DirectoryInfo ( Path.Combine ( target.FullName, findData.cFileName ) );
                        }
                    } while ( NativeWin32.FindNextFile ( hFindFile, out findData ) );
                }
            }
            
        }

        public static IEnumerable<FileInfo> EnumerateFiles ( this DirectoryInfo target )
        {
           return EnumerateFiles ( target, "*" );
        }

        public static IEnumerable<FileInfo> EnumerateFiles ( this DirectoryInfo target, string searchPattern )
        {
            string searchPath = Path.Combine ( target.FullName, searchPattern );
            NativeWin32.WIN32_FIND_DATA findData;
            using ( NativeWin32.SafeSearchHandle hFindFile = NativeWin32.FindFirstFile ( searchPath, out findData ) )
            {
                if ( !hFindFile.IsInvalid )
                {
                    do
                    {
                        if ( ( findData.dwFileAttributes & FileAttributes.Directory ) == 0 && findData.cFileName != "." && findData.cFileName != ".." )
                        {
                            yield return new FileInfo ( Path.Combine ( target.FullName, findData.cFileName ) );
                        }
                    } while ( NativeWin32.FindNextFile ( hFindFile, out findData ) );
                }
            }

        }
    }
}

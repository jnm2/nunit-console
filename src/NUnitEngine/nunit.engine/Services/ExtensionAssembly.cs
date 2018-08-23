﻿// ***********************************************************************
// Copyright (c) 2016 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System.IO;
using System.Reflection;
using NUnit.Engine.Internal.Metadata;

namespace NUnit.Engine.Services
{
    public class ExtensionAssembly
    {
        public ExtensionAssembly(string filePath, bool fromWildCard)
        {
            FilePath = filePath;
            FromWildCard = fromWildCard;
        }

        public string FilePath { get; private set; }
        public bool FromWildCard { get; private set; }

        private IAssemblyMetadataProvider _metadata;

        internal IAssemblyMetadataProvider Metadata
        {
            get => _metadata ?? (_metadata = AssemblyMetadataProvider.Create(FilePath));
        }

        /// <summary>
        /// IsDuplicateOf returns true if two assemblies have the same name.
        /// </summary>
        public bool IsDuplicateOf(ExtensionAssembly other)
        {
            return Metadata.AssemblyName.Name == other.Metadata.AssemblyName.Name;
        }

        /// <summary>
        /// IsBetterVersion determines whether another assembly is
        /// a better (higher) version than the current assembly.
        /// It is only intended to be called if IsDuplicateOf
        /// has already returned true.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsBetterVersionOf(ExtensionAssembly other)
        {
#if DEBUG
            if (!IsDuplicateOf(other))
                throw new NUnitEngineException("IsBetterVersionOf should only be called on duplicate assemblies");
#endif

            var version = Metadata.AssemblyName.Version;
            var otherVersion = other.Metadata.AssemblyName.Version;

            if (version > otherVersion)
                return true;

            if (version < otherVersion)
                return false;

            // Versions are equal, override only if this one was specified exactly while the other wasn't
            return !FromWildCard && other.FromWildCard;
        }

        internal string ResolveAssemblyPath(AssemblyName assemblyName)
        {
            return AssemblyMetadataProvider.TryResolveAssemblyPath(assemblyName.Name, Path.GetDirectoryName(FilePath))
                ?? AssemblyMetadataProvider.TryResolveAssemblyPath(assemblyName.Name, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }
    }
}

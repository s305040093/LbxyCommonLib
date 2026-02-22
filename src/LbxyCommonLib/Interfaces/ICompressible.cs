// <copyright file="ICompressible.cs" company="Lbxy">
// Copyright (c) 2026 Lbxy
// </copyright>
namespace LbxyCommonLib.ListCompression.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a compressible element contract. Elements are compared for compression using <see cref="object.Equals(object)"/>.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public interface ICompressible<T>
    {
    }
}

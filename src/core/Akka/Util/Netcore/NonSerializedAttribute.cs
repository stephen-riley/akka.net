﻿//-----------------------------------------------------------------------
// <copyright file="NonSerializedAttribute.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

#if !SERIALIZATION
namespace System
{
    public class NonSerializedAttribute : Attribute
    {
    }
}
#endif
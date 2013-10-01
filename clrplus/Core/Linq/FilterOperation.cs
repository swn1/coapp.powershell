//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Copyright (c) 2010-2013 Garrett Serack and CoApp Contributors. 
//     Contributors can be discovered using the 'git log' command.
//     All rights reserved.
// </copyright>
// <license>
//     The software is licensed under the Apache 2.0 License (the "License")
//     You may not use the software except in compliance with the License. 
// </license>
//-----------------------------------------------------------------------

namespace ClrPlus.Core.Linq {
    public enum FilterOperation {
        Eq,
        Equals = Eq,
        Lt,
        LessThan = Lt,
        Gt,
        GreaterThan = Gt,
        Lte,
        LessThanOrEqual = Lte,
        Gte,
        GreaterThanOrEqual = Gte,
        Contains,
        Neq,
        NotEquals,
    }
}
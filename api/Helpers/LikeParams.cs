﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class LikeParams : PaginationParams
    {
        public int UserId { get; set; }

        public string Predicate { get; set; }

    }
}

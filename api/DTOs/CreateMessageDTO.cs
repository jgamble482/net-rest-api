﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class CreateMessageDTO
    {
        public string RecipientUsername { get; set; }

        public string Content { get; set; }

    }
}

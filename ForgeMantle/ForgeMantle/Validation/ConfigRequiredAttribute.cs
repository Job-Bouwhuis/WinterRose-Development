﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeMantle.Validation;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ConfigRequiredAttribute : Attribute
{
}


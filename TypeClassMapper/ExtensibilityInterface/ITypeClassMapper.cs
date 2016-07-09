﻿using System;
using System.Collections.Generic;

namespace nutility
{
  /// <summary>
  /// By now, if query to existing mappings is required, then casting to this interface is in order.
  /// </summary>
  public interface ITypeClassMapper : IServiceProvider
  {
    /// <summary>
    /// Existing type-class mappings.
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Mappings { get; }
  }
}
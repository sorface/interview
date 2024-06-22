using System.Collections;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.Common;

/// <summary>
/// Extensions methods for collection.
/// </summary>
public static class CollectionExt
{
    /// <summary>
    /// Convert collection to ActionResult.
    /// </summary>
    /// <param name="self">Collection.</param>
    /// <typeparam name="TCollection">Collection type.</typeparam>
    /// <returns>Action result.</returns>
    public static async Task<ActionResult<TCollection>> ToActionResultAsync<TCollection>(this Task<TCollection> self)
        where TCollection : ICollection
    {
        var result = await self;
        return result.ToActionResult();
    }

    /// <summary>
    /// Convert collection to ActionResult.
    /// </summary>
    /// <param name="self">Collection.</param>
    /// <typeparam name="TCollection">Collection type.</typeparam>
    /// <returns>Action result.</returns>
    public static ActionResult<TCollection> ToActionResult<TCollection>(this TCollection self)
        where TCollection : ICollection
    {
        if (self.Count == 0)
        {
            return new NoContentResult();
        }

        return new OkObjectResult(self);
    }
}

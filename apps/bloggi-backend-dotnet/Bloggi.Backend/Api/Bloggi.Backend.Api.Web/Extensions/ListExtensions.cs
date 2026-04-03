namespace Bloggi.Backend.Api.Web.Extensions;

/// <summary>
/// Provides extension methods for <see cref="List{T}"/> to enhance functionality.
/// </summary>
public static class ListExtensions
{
    /// <param name="list">The list to which the item may be added.</param>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    extension<T>(List<T> list)
    {
        /// <summary>
        /// Adds the specified item to the list if it is not null.
        /// </summary>
        /// <param name="item">The item to potentially add to the list.</param>
        public void AddIfNotNull(T? item)
        {
            if (item is not null)
                list.Add(item);
        }

        /// <summary>
        /// Adds the specified range of items to the list, excluding any null items.
        /// </summary>
        /// <param name="items">The collection of items to potentially add to the list.</param>
        public void AddRangeIfNotNull(IEnumerable<T> items)
        {
            items.Where(x => x is not null).ToList().ForEach(list.Add!);
        }
    }
}
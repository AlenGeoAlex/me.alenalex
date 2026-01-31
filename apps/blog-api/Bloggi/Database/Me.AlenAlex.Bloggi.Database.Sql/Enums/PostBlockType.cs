using System.Diagnostics.CodeAnalysis;

namespace Me.AlenAlex.Bloggi.Database.Sql.Enums;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PostBlockType
{
    text,
    markdown,
    media,
    code,
    html,
    card_content,
    embed
}
// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

/// <summary>
///     Represents an entry in the <see cref="TemplateConfigurationWindow.templateListView" />, with references to the corresponding
///     settings objects
/// </summary>
/// <param name="TemplateName">The name of the template. Will be used as the display text in the list</param>
/// <param name="ViewSettings">The view copy of the template</param>
/// <param name="BaseSettings">The base copy of the template</param>
public record TemplateConfigurationListItem( string TemplateName, TemplateSettings ViewSettings, TemplateSettings BaseSettings )
{
    /// <summary>
    ///     Gets or sets a reference to the base copy of the template
    /// </summary>
    /// <remarks>
    ///     Used primarily for comparison against <see cref="ViewSettings" />, to determine if changes have been made
    /// </remarks>
    public TemplateSettings BaseSettings { get; set; } = BaseSettings;

    /// <summary>
    ///     Gets <see cref="ViewSettings" /> != <see cref="BaseSettings" />
    /// </summary>
    public bool IsModified => ViewSettings != BaseSettings;

    /// <summary>
    ///     Gets or sets a reference to the view copy of the template
    /// </summary>
    /// <remarks>
    ///     This is the reference used for display purposes
    /// </remarks>
    public TemplateSettings ViewSettings { get; set; } = ViewSettings;

    /// <inheritdoc />
    public override string ToString( )
    {
        return TemplateName;
    }
}

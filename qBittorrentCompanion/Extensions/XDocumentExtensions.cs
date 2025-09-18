using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace qBittorrentCompanion.Extensions
{
    /// <summary>
    /// Adds several chainable ease-of-use methods targetting nodes by id
    /// </summary>
    public static class XDocumentExtensions
    {
        /// <summary>
        /// Walks through all descendants and returns the first node where the given id parameter is matched
        /// <br/><em>id should be unique, there never should be more than one</em>
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static XElement? GetElementById(this XDocument doc, string id)
        {
            return doc.Descendants()
                .FirstOrDefault(e => e.Attribute("id")?.Value == id);
        }

        /// <summary>
        /// Sets the stroke attribute to the given `color` parameter on the node matching the given `id`.
        /// <br/>Uses <see cref="GetElementById"/> internally
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="id"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static XDocument SetSvgStroke(this XDocument doc, string id, string color)
        {
            doc.GetElementById(id)?
                .SetAttributeValue("stroke", color);

            return doc;
        }

        /// <summary>
        /// Gets the &lt;stop stop-color=""/&gt; elements from a gradient matching the given `id`
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="gradientId"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetStopColorsFromGradientById(this XDocument doc, string gradientId)
        {
            return doc.GetElementById(gradientId)
                ?.Descendants()
                .Where(e => e.Name.LocalName == "stop" && e.Attribute("stop-color")?.Value is not null) ?? [];
        }

        /// <summary>
        /// Sets the stroke attribute to the given `color` parameter on the <stop> element matching the given `stopIndex`.
        /// <br/>This is on the gradient matching the given `gradientId`
        /// <br/>Uses <see cref="GetElementById"/> internally
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="gradientId"></param>
        /// <param name="stopIndex"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static XDocument SetSvgGradientStop(this XDocument doc, string gradientId, int stopIndex, string color) 
        {
            GetStopColorsFromGradientById(doc, gradientId)
                ?.ElementAtOrDefault(stopIndex)
                ?.SetAttributeValue("stop-color", color);

            return doc;
        }
    }
}

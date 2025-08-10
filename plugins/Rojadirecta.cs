package com.cloudstream.rojadirecta

import com.lagradost.cloudstream3.*
import com.lagradost.cloudstream3.utils.*
import org.jsoup.Jsoup

class RojadirectaPlugin : MainPlugin() {
    // Basic plugin info
    override var mainUrl = "https://www.rojadirectaenvivo.pl"
    override var name = "Rojadirecta"
    override val supportedTypes = setOf(TvType.Live)
    override val hasMainPage = false

    // Main scraper function
    override suspend fun load(url: String): LoadResponse {
        val doc = app.get(url).document
        
        // Extract title and thumbnail
        val title = doc.selectFirst("h1.entry-title")?.text() 
            ?: "Live Football Stream"
        val poster = doc.selectFirst("div.entry-content img")?.attr("src")

        // Find all potential video iframes
        val iframes = doc.select("iframe")

        // Prepare links list
        val links = iframes.mapNotNull { iframe ->
            val src = iframe.attr("src").takeIf { it.isNotBlank() }
            src?.let {
                EpisodeLink(
                    "Stream Link",
                    it,
                    url,
                    Qualities.Unknown.value
                )
            }
        }

        return LiveStreamLoadResponse(
            title,
            url,
            this.name,
            TvType.Live,
            poster,
            null,
            emptyList(),
            links
        )
    }

    // Direct link extractor (for embedded players)
    override suspend fun loadLinks(
        data: String,
        isCasting: Boolean
    ): List<EpisodeLink> {
        val doc = app.get(data).document
        return doc.select("iframe").mapNotNull { iframe ->
            val src = iframe.attr("src").takeIf { it.isNotBlank() }
            src?.let {
                EpisodeLink(
                    "Embedded Player",
                    it,
                    data,
                    Qualities.Unknown.value
                )
            }
        }
    }
}

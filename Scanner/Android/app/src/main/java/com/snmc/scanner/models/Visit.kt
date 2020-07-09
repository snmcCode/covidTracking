package com.snmc.scanner.models

import java.util.*

data class Visit(val visitorId: UUID,
                 val organization: String,
                 val door: String,
                 val direction: String)
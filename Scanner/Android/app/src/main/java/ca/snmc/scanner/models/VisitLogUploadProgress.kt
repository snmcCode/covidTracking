package ca.snmc.scanner.models

data class VisitLogUploadProgress(
    var progress: Int = 0,
    var timeout: Boolean = false,
    var uploadedItems: Int = 0,
    var totalItems: Int = 0
) {
}
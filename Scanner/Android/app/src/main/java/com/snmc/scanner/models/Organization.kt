package com.snmc.scanner.models

data class Organization(val id: Int,
                        val name: String,
                        val contactName: String,
                        val contactNumber: String,
                        val contactEmail: String,
                        val loginName: String,
                        val loginSecretHash: String)
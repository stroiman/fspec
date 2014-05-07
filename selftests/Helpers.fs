module FSpec.SelfTests.Helpers

let stringBuilderPrinter builder =
    fun color msg ->
        Printf.bprintf builder "%s" msg
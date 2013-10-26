FSpec
=====

_RSpec inspired test framework for F#_

The aim of this project is to provide a test framework to the .NET platform
having the same elegance as the RSpec framework has on Ruby.

The code is still in a very exploratory state, so the actual syntax for writing
tests may change. 

The framework is currently self testing, i.e. the framework is used to test
itself.

I am currently developing the framework using mono and a simple text editor.
Therefore no F# project files or msbuild compatible scripts. But I would not
object if a contributer would wrap the code into Visual Studio project files.

## Usage (subject to change) ##

Create an assembly containing your specs. Place your specs in a module, and
assign the spec code to the variable _spec_. Pass the name of the assembly to
the fspec runner command line.

```fsharp
module MySpecModule

let specs =
    describe "Some feature" <| fun() ->
        describe "In some context" <| fun() ->
            it "has some specific behaviour" <| fun() ->
                ()
            it "has some other specific behavior" <| fun() ->
                ()
        describe "In some other context" <| fun() ->
            it "has some completely different behavior" <| fun() ->
                ()
```

### General setup/teardown code ###

The functions _before_/_after_ can be used to hold general setup/teardown code.

```fsharp
let specs =
    describe "Some feature" <| fun() ->
        before fun() ->
            ()
        after fun() ->
            ()
        it "Has some behavior" <| fun() ->
            ()
```

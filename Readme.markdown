FSpec
=====

_RSpec inspired test framework for F#_

The aim of this project is to provide a test framework to the .NET platform
having the same elegance as the RSpec framework has on Ruby.

The code is still in a very exploratory state, so the actual syntax for writing
tests may change. 

The framework is currently self testing, i.e. the framework is used to test
itself.

## Usage (subject to change) ##

Create an assembly containing your specs. Place your specs in a module, and
assign the spec code to the value _spec_. Pass the name of the assembly to
the fspec runner command line.

```fsharp
module MySpecModule

let specs =
    describe "Some feature" (fun() ->
        describe "In some context" (fun() ->
            it "has some specific behaviour" (fun _ ->
                ()
            )
            it "has some other specific behavior" (fun _ ->
                ()
            )
        )
        describe "In some other context" (fun() ->
            it "has some completely different behavior" (fun _ ->
                ()
            )
        )
```

If you find the paranthesis noisy, you can use the backward pipe operator

```fsharp
let specs =
    describe "Some feature" <| fun _ ->
        describe "In some context" <| fun _ ->
            it "has some specific behaviour" <| fun _ ->
                ()
            it "has some other specific behaviour" <| fun _ ->
                ()
```

### Pending tests ###

You can use the function _pending_ as the test body to indicate a test that
needs to be written.

```fsharp
let specs =
    describe "Some feature" (fun _ ->
        it "has some specific behaviour" pending
        it "has some other specific behaviour" pending
    )
```

This allows you to quickly describe required functionality without being forced
to write a full test up front. 

The test runner reports pending tests, thus you will know if you have more work
to do before the feature is complete.

### General setup/teardown code ###

The functions _before_/_after_ can be used to hold general setup/teardown code.

```fsharp
let specs =
    describe "Some feature" <| fun _ ->
        before fun _ ->
            ()
        after fun _ ->
            ()
        it "Has some behavior" <| fun _ ->
            ()
```

### Initialization ###

You can use the function _init_ to initialize an object once, and only once,
during the execution of a test

```fsharp
let specs =
    describe "Some feature" (fun _ ->
        let dependency = init (fun _ -> new Dependency())

        it "has some specific behavior" (fun _ ->
            dependency().SetupInSomeStat()
            dependency().AndSomeOtherChildStat()
            ...
        )

        it "has some other specific behavior" (fun _ ->
            dependency().SetupInSomeStat()
            dependency().AndSomeOtherChildStat()
            ...
        )
    )
```

In the above example, the _Dependency_ class will only be instantiated once for
each test. This is also true if it is accessed from setup code inside a _before_
block.

## What I don't like ##

What I currently don't like is the handling of mutable data. Testing is in
itself not a problem that is necessarily best solved in a functional programming
domain. This is particularly true when multiple tests need to reuse a fixture
set up using common setup code. This forces the introduction of imperative
programming with the current fspec framework implementation.

And although F# is a multi-paradigm language, adding imperative programming just
doesn't appear as 'clean' as a purely functional. This becomes even worse with
data setup in a _before_ body, because in F#, you cannot access mutable values
inside closures. Therefore you have to introduce references.

```fsharp
describe "Some context" (fun _ ->
    let counter = ref 0

    before (fun _ -> 
        counter := 0
        let callback _ ->
            counter := !counter + 1
        registerCallback callback
    )

    it "Calls back twice" (fun _ ->
        do_something_that_triggers_the_callback
        (!counter) |> should equal 2
    )
)
```

The use of references introduces quite a lot of language noise, imho.

I have an idea for an alternate DSL that can clean up some of the noise, but at
the expense of other noise, unfortunately.

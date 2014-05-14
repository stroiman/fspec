FSpec
=====

_RSpec inspired test framework for F#_

The aim of this project is to provide a test framework to the .NET platform
having the same elegance as the RSpec framework has on Ruby.

You can easily use this framework to test C# code.

Currently the following features are supported

 * Nested example groups/contexts
 * Setup/teardown in example groups
 * Test context - can be used to pass data from setup to test
 * Metadata on individual examples or example groups (accessible from setup)
 * Assertion framework
 * Implicit subject
 * Automatically disposing _IDisposable_ instances

Ideas for future improvements (prioritized list)

 * Support for missing metadata, i.e. test context can try to retrieve meta
   data that may or may not have been initialized.
 * Better error messages when context/meta data does not exist, or is of
   incorrect type.
 * Better support for batch building examples.
 * One liner verifications using expressions (I want this)
 * Context data and meta data keys can be other types than strings, e.g.
   discriminated unions, partly to avoid name clashes.
 * Global setup/teardown code, useful for clearing database between tests.

The framework is self testing, i.e. the framework is used to test itself.

## Possible Future Changes ##

As the FSpec tool is still quite new and haven't really been used in many
projects (only a few of my own), there is a risk that the API could change. 

The DSL for building examples and example groups have already undergone a big
change, but I'm quite happy with the way it looks now. I don't expect any major
changes to this, only minor additions, or possibly modifications to how meta
data is applied to examples and example groups.

I'm not currently happy with the matcher API, and I am experimenting with a new
API that allows better support for composition, and custom matchers. So this
could change.

But you can use any assertion framework, e.g. unqoute, for matchers.

## General syntax ##

Create an assembly containing your specs. Place your specs in a module, and
assign the spec code to the value _spec_. Pass the name of the assembly to
the fspec runner command line.

```fsharp
module MySpecModule

let specs =
    describe "Some component" [
        describe "Some feature" [
            context "In some context" [
                it "has some specific behaviour" (fun _ ->
                    ()
                )
                it "has some other specific behavior" (fun _ ->
                    ()
                )
            ]
            context "In some other context" [
                it "has some completely different behavior" (fun _ ->
                    ()
                )
            ]]]
```

If you find the paranthesis noisy, you can use the backward pipe operator

```fsharp
let specs =
    describe "Some feature" [
        context "In some context" [
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
    describe "Some feature" [
        it "has some specific behaviour" pending
        it "has some other specific behaviour" pending
    ]
```

This allows you to quickly describe required functionality without being forced
to write a full test up front. 

The test runner reports pending tests, thus you will know if you have more work
to do before the feature is complete. The test runner will not fail because of
pending tests.

### General setup/teardown code ###

The functions _before_/_after_ can be used to hold general setup/teardown code.

```fsharp
let specs =
    describe "Some feature" <| fun _ ->
        before <| fun _ ->
            ()
        after <| fun _ ->
            ()
        it "Has some behavior" <| fun _ ->
            ()
```

## Test Context ##

Setup, teardown, and test functions all receive a _TestContext_ parameter. Test
metadata can be received from this context, and specific test data can be
stored in the context. The latter is useful if the test needs to use data
created in the setup.

The context data is accessible using the ? operator.

```fsharp
let specs =
    describe "createUser function" [
        before <| fun _ ->
            ctx?user <- createUser "John" "Doe"
        it "sets the first name" <| fun ctx ->
            ctx?user |> User.firstName |> should equal "John"
        it "sets the last name" <| fun ctx ->
            ctx?user |> User.lastName |> should equal "Doe"
    ]
```

Where other test frameworks relies on class member fields to share data
between, e.g. general setup and test code, the _TestContext_ is the place to
store it in FSpec.

If you get a compiler error saying that the it cannot infer the type, use
the generic _Get<'T>_ function instead.

### Automatically Disposal ###

If you add an object that implements _IDisposable_ to the test context, the
object will automatically be disposed when the test run has finished. 

```fsharp
let specs =
    describe "The data access layer" [
        before (fun ctx -> ctx?connection <- craeteDatabaseConnection () )

        it "uses the connection" ...
    ]
```

The database connection will in this case automatically be disposed.

```fsharp
let specs =
    describe "createUser function" [
        before <| fun _ ->
            ctx?user = createUser "John" "Doe"
        it "sets the first name" <| fun ctx ->
            let user = ctx.Get<User> "user"
            user.FirstName |> should equal "John"
    ]
```
  
### Implicit subject ###

A special context variable, _Subject_, can be used to reference the thing under
test. It can be retrieved using the _getSubject_ function.

```fsharp
let specs =
    describe "createUser function" [
        subject <| fun _ -> createuser "john" "doe"

        it "sets the first name" <| fun ctx ->
            ctx |> getSubject |> User.firstName |> should equal "John"
        it "sets the last name" <| fun ctx ->
            ctx |> getSubject |> User.lastName |> should equal "Doe"
    ]
```

_getSubject_ is generic, taking the actual type of subject as type argument,
but often F# type inference will figure out the type argument based on the
usage.

The subject can also be a function.

```fsharp
let specs =
    describe "createUser function, when user already exists" [
        ...
        subject <| fun _ -> 
            (fun () -> CreateAndSaveNewUser())
        it "should fail" <| fun ctx ->
            ctx |> getSubject |> should fail
```

### Test metadata ###

You can associate metadata to an individual example, or an example group. The
syntax is currently a strange set of operators. The metadata is basically a map
of _string_ keys and _obj_ values.

The metadata getter is generic, but will fail at runtime if the actual data is
not of the correct type.

Metadata can be useful when you want to modify a general setup in a more
specific context.

```fsharp
let specs =
    describe "Register new user feature" [
        before (fun ctx ->
            let user = ctx.metadata?existing_user
            Mock<IUserRepository>()
                .Setup(fun x -> <@ x.FindByEmail(email) @>)
                .Returns(user)
                .Create()
            |> // do something with the mock
        )

        ("existing_user" ++ null) ==>
        context "when no user exists" [
            it "succeeds" (...)
        ]

        ("existing_user" ++ createValidUser()) ==>
        context "when a user has already been registered with that email" [
            it "fails" (...)
        ]

        context "an example with many pieces of metadata" [
            ("data1" ++ 42 |||
             "data2" ++ "Yummy" |||
             "data3" ++ Some [1;2;3]) ==>
            it "can easily specify a lot of metadata" (fun _ -> ())
        ]
    ]
```

The _++_ operator combines creates a collection of metadata, and the _|||_
operator combines two collections of metadata. The _==>_ operator passes a
collection of metadata to the following example or example group.

Metadata with the same name on a child example group will override the value of
the parent group, and metadata on an example will override that of the group.

## Assertion framework ##

FSpec has it's own set of assertions (not very complete currently). The
assertions are typed, so the actual value must be of the correct type

```fsharp
5 |> should equal 5 // pass
5 |> should be.greaterThan 6 // fail
5 |> should equal 5.0 // does not compile.
"blah blah" |> should beInstanceOf<string> // pass
42 |> should beInstanceOf<string> // fail
"foobar" |> should matchRegex "ooba" // pass
```

The strongly typed matchers also works with context data.

```fsharp
ctx?user |> User.firstName |> should equal "John"
ctx.metadata?email |> should equal "john.doe@example.com"
```

This will automatically cause the data collections to try to retrieve the
object of the correct type.

### Writing new assertions ###

Is possible, and soon to be documented.

## Extending Test Context ##

Common test functionality can be created by writing extensions to the
TestContext type. E.g. here, where FSpec is used to test a typical C#
dependency injection architectural project.

```fsharp
module MyApplicationSpecs.TestHelpers
open FSpec.Core

type TestContext with
    member self.AutoMocker =
        match self.TryGet "auto_mocker" with
        | Some mocker -> mocker
        | None ->
              let mocker = AutoMocker()
              self.Set "auto_mocker" mocker
              mocker
    member self.GetMock<'T> () = self.AutoMocker.GetMock<'T> ()
    member self.Get<'T> () = self.AutoMocker.Get<'T> ()
```

Open the _TestHelpers_ module from any test module where you need to test a
component with mocked dependencies, and you will have access to the _Get<'T>()_
and _GetMock<'T>()_ methods.

### Batch building examples ###

Because examples are created at runtime, you can use _List_ operations to
generate batches of test cases.

Although this example is a bit noisy with paranthesis, it shows that it can
be done with the current api.

```fsharp
let specs =
    describe "Email validator" (
        (["user@example.com"
          ...
          "dotted.user@example.com"]
         |> List.map (fun email ->
            it (sprintf "validates email: %s" email) (fun _ ->
                email |> validateEmail |> should equal true))
        ) @ 
        (["user@example";
          ...
          "user@.com"]
         |> List.map (fun email ->
            it (sprintf "does not validate email: %s" email) (fun _ ->
                email |> validateEmail |> should equal false))
        )
    )
```

## Running the tests ##

You can either use the console runner for running the specs. Or - create you
spec assembly as a console application, and use this following piece of code as
the main function

```fsharp
[<EntryPoint>]
let main args =
    System.Reflection.Assembly.GetExecutingAssembly()
    |> getSpecsFromAssembly
    |> runSpecs
    |> toExitCode
```

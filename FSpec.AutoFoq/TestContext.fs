namespace FSpec.AutoFoq
open FSpec.AutoFoq.Internals
open FSpec

[<AutoOpen>]
module Extensions =
    type TestContext with
        member self.AutoMocker = self.GetOrDefault "auto_mocker" (fun _ -> AutoMocker())
        member self.GetMock<'T> () = self.AutoMocker.Get<'T> ()
        member self.GetInstance<'T> () = self.AutoMocker.Get<'T> ()
        member self.Inject<'T> x = self.AutoMocker.Inject<'T> x
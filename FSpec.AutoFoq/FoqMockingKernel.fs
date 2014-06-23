namespace FSpec.AutoFoq.Internals
open Foq
open Ninject
open Ninject.Modules
open Ninject.MockingKernel  
open Ninject.Activation
open Ninject.Components
open Ninject.Activation.Caching

type FoqMockProvider() =
    inherit NinjectComponent()

    member this.Create<'T when 'T : not struct> () =
        Mock<'T>().Create()

    interface IMockProviderCallbackProvider with
        member this.GetCreationCallback () =
            let result = fun (x : IContext) -> this :> IProvider
            System.Func<IContext, IProvider>(result)

    interface IProvider with
        member this.Create(ctx : IContext) =
            let requestedType = ctx.Request.Service
            let create = this.GetType().GetMethod("Create")
            let actualMethod = create.MakeGenericMethod(requestedType)
            actualMethod.Invoke(this, [||])

        member this.get_Type () = typeof<Foq.Mock>

type FoqModule() =
    inherit NinjectModule()

    override this.Load() = 
        this.Kernel.Components.Add<IMockProviderCallbackProvider, FoqMockProvider>()

type FoqMockingKernel() as this =
    inherit MockingKernel()

    do this.Load(new FoqModule())

type AutoMocker() =
    let mockingKernel = new FoqMockingKernel()
    let mutable injectedTypes = []

    member x.Reset () =
        mockingKernel.Reset()
        injectedTypes
        |> List.iter (fun t -> mockingKernel.Unbind(t))
        injectedTypes <- []

    member x.Get<'T> () =
        mockingKernel.Get<'T> ()

    member x.Inject<'T> (instance : 'T) =
        mockingKernel.Bind<'T>().ToConstant(instance) |> ignore
        injectedTypes <- typeof<'T> :: injectedTypes

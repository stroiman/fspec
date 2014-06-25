module AutoFoqSpecs
open FSpec.Dsl
open FSpec.Matchers
open FSpec.AutoFoq
open Foq

module Helpers =
    type TestEntity = { Id : int }
    with static member createWithId id = { Id = id }

    type ITestDataAccess =
        abstract member Load : int -> TestEntity
        abstract member Save : TestEntity -> unit

    type TestApplicationService(dataAccess : ITestDataAccess) =
        member __.Get (id:int) = ()
        member __.Save entity = dataAccess.Save entity
open Helpers

let specs =
    describe "FSpec.AutoFoq" [
        it "allows you to setup code first" <| fun ctx ->
            let entity = TestEntity.createWithId 42
            ctx.InitMock<ITestDataAccess>(fun mock ->
                    mock.Setup(fun x -> <@ x.Load 42 @>)
                        .Returns(entity))
            let service = ctx.GetInstance<TestApplicationService>()
            let entity = service.Get 42
            entity.Should (equal entity)

        it "allows you to inject a configured mock" <| fun ctx ->
            let entity = TestEntity.createWithId 42
            ctx.Inject <|
                Mock<ITestDataAccess>()
                    .Setup(fun x -> <@ x.Load 42 @>)
                    .Returns(entity)
                    .Create()
            let service = ctx.GetInstance<TestApplicationService>()
            let entity = service.Get 42
            entity.Should (equal entity)

        it "allows you to retrieve the mocked object after execution" <| fun ctx ->
            let entity = TestEntity.createWithId 42
            let service = ctx.GetInstance<TestApplicationService>()
            service.Save entity

            let dataAccess = ctx.GetInstance<ITestDataAccess>()
            verify <@ dataAccess.Save(entity) @> once
    ]
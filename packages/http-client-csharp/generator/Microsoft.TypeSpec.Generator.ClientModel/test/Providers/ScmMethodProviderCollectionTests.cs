// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.TypeSpec.Generator.ClientModel.Providers;
using Microsoft.TypeSpec.Generator.Input;
using Microsoft.TypeSpec.Generator.Input.Extensions;
using Microsoft.TypeSpec.Generator.Primitives;
using Microsoft.TypeSpec.Generator.Providers;
using Microsoft.TypeSpec.Generator.Snippets;
using Microsoft.TypeSpec.Generator.Tests.Common;
using NUnit.Framework;

namespace Microsoft.TypeSpec.Generator.ClientModel.Tests.Providers
{
    internal class ScmMethodProviderCollectionTests
    {
        private static readonly InputModelType _spreadModel = InputFactory.Model(
            "spreadModel",
            usage: InputModelTypeUsage.Spread,
            properties:
            [
                InputFactory.Property("p2", InputPrimitiveType.String, isRequired: true),
            ]);

        // Validate that the default method collection consists of the expected method kind(s)
        [TestCaseSource(nameof(DefaultCSharpMethodCollectionTestCases))]
        public void TestDefaultCSharpMethodCollection(InputServiceMethod serviceMethod)
        {
            var inputClient = InputFactory.Client("TestClient", methods: [serviceMethod]);

            MockHelpers.LoadMockGenerator(
                createCSharpTypeCore: (inputType) => new CSharpType(typeof(bool)));

            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            Assert.IsNotNull(client);
            var methodCollection = new ScmMethodProviderCollection(serviceMethod, client!);
            Assert.IsNotNull(methodCollection);
            Assert.AreEqual(4, methodCollection.Count);

            var method = methodCollection![0];
            var signature = method.Signature;
            Assert.IsNotNull(signature);
            var operation = serviceMethod.Operation;
            Assert.AreEqual(operation.Name.ToIdentifierName(), signature.Name);

            var parameters = signature.Parameters;
            Assert.IsNotNull(parameters);
            Assert.AreEqual(operation.Parameters.Count + 1, parameters.Count);

            var convenienceMethod = methodCollection.FirstOrDefault(m
                => !m.Signature.Parameters.Any(p => p.Name == "content")
                    && m.Signature.Name == $"{operation.Name.ToIdentifierName()}");
            Assert.IsNotNull(convenienceMethod);
            Assert.AreEqual(serviceMethod, convenienceMethod!.ServiceMethod);

            var convenienceMethodParams = convenienceMethod!.Signature.Parameters;
            Assert.IsNotNull(convenienceMethodParams);

            var spreadInputParameter = operation.Parameters.FirstOrDefault(p => p.Kind == InputParameterKind.Spread);
            if (spreadInputParameter != null)
            {
                var spreadModelProperties = _spreadModel.Properties;
                // model properties + 2 (parameter and cancellation token)
                Assert.AreEqual(spreadModelProperties.Count + 2, convenienceMethodParams.Count);
                Assert.AreEqual("p1", convenienceMethodParams[0].Name);
                Assert.AreEqual(spreadModelProperties[0].Name, convenienceMethodParams[1].Name);
            }
        }

        [TestCaseSource(nameof(DefaultCSharpMethodCollectionTestCases))]
        public void ConvenienceMethodsHaveOptionalCancellationToken(InputServiceMethod serviceMethod)
        {
            var inputClient = InputFactory.Client("TestClient", methods: [serviceMethod]);

            MockHelpers.LoadMockGenerator(
                createCSharpTypeCore: (inputType) => new CSharpType(typeof(bool)));

            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            Assert.IsNotNull(client);
            var methodCollection = new ScmMethodProviderCollection(serviceMethod, client!);
            Assert.IsNotNull(methodCollection);
            Assert.AreEqual(4, methodCollection.Count);

            var operation = serviceMethod.Operation;
            var asyncConvenienceMethod = methodCollection.FirstOrDefault(m
                => !m.Signature.Parameters.Any(p => p.Name == "content")
                    && m.Signature.Name == $"{operation.Name.ToIdentifierName()}Async");
            Assert.IsNotNull(asyncConvenienceMethod);

            var asyncConvenienceMethodParameters = asyncConvenienceMethod!.Signature.Parameters;
            Assert.IsNotNull(asyncConvenienceMethodParameters);

            var lastParameter = asyncConvenienceMethodParameters.Last();
            Assert.IsTrue(lastParameter.Type.Equals(typeof(CancellationToken)));
            Assert.IsFalse(lastParameter.Type.IsNullable);
            Assert.AreEqual(Snippet.Default, lastParameter.DefaultValue);

            var syncConvenienceMethod = methodCollection.FirstOrDefault(m
                => !m.Signature.Parameters.Any(p => p.Name == "content")
                   && m.Signature.Name == operation.Name.ToIdentifierName());
            Assert.IsNotNull(syncConvenienceMethod);

            var syncConvenienceMethodParameters = syncConvenienceMethod!.Signature.Parameters;
            Assert.IsNotNull(syncConvenienceMethodParameters);

            lastParameter = syncConvenienceMethodParameters.Last();
            Assert.IsTrue(lastParameter.Type.Equals(typeof(CancellationToken)));
            Assert.IsFalse(lastParameter.Type.IsNullable);
            Assert.AreEqual(Snippet.Default, lastParameter.DefaultValue);
        }

        [Test]
        public void ListMethodWithNoPaging()
        {
            MockHelpers.LoadMockGenerator();
            var inputModel = InputFactory.Model("cat", properties:
            [
                InputFactory.Property("color", InputPrimitiveType.String, isRequired: true),
            ]);
            var modelType = ScmCodeModelGenerator.Instance.TypeFactory.CreateCSharpType(inputModel);
            var response = InputFactory.OperationResponse(
                [200],
                InputFactory.Array(inputModel));
            var operation = InputFactory.Operation("getCats", responses: [response]);
            var inputServiceMethod = InputFactory.BasicServiceMethod("Test", operation);
            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            Assert.IsNotNull(client);

            // there should be no CollectionResultDefinition
            Assert.IsFalse(ScmCodeModelGenerator.Instance.OutputLibrary.TypeProviders.Any(t => t is CollectionResultDefinition));

            var methodCollection = new ScmMethodProviderCollection(inputClient.Methods.First(), client!);
            Assert.IsNotNull(methodCollection);
            Assert.AreEqual(4, methodCollection.Count);
            var listMethod = methodCollection.FirstOrDefault(
                m => !m.Signature.Parameters.Any(p => p.Name == "options") && m.Signature.Name == "GetCats");
            Assert.IsNotNull(listMethod);
            var signature = listMethod!.Signature;

            var expectedReturnType = new CSharpType(typeof(ClientResult<>), new CSharpType(typeof(IReadOnlyList<>), modelType!));
            Assert.IsTrue(signature.ReturnType!.Equals(expectedReturnType));
        }

        [Test]
        public void ListMethodWithImplicitPaging()
        {
            var pagingMetadata = InputFactory.PagingMetadata(
                ["items"],
                null,
                null);
            var inputModel = InputFactory.Model("cat", properties:
            [
                InputFactory.Property("color", InputPrimitiveType.String, isRequired: true),
            ]);

            var response = InputFactory.OperationResponse(
                [200],
                InputFactory.Model(
                    "page",
                    properties: [InputFactory.Property("cats", InputFactory.Array(inputModel))]));
            var operation = InputFactory.Operation("getCats", responses: [response]);
            var inputServiceMethod = InputFactory.PagingServiceMethod("Test", operation, pagingMetadata: pagingMetadata);
            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);

            MockHelpers.LoadMockGenerator(inputModels: () => [inputModel], clients: () => [inputClient]);
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            Assert.IsNotNull(client);

            // there should be a CollectionResultDefinition
            var collectionResultDefinition = ScmCodeModelGenerator.Instance.OutputLibrary.TypeProviders.FirstOrDefault(
               t => t is CollectionResultDefinition);
            Assert.IsNotNull(collectionResultDefinition);

            var methodCollection = new ScmMethodProviderCollection(inputClient.Methods.First(), client!);
            Assert.IsNotNull(methodCollection);
            Assert.AreEqual(4, methodCollection.Count);

            var listMethod = methodCollection.FirstOrDefault(
                m => !m.Signature.Parameters.Any(p => p.Name == "options") && m.Signature.Name == "GetCats");
            Assert.IsNotNull(listMethod);

            var signature = listMethod!.Signature;
            var expectedReturnType = new CSharpType(typeof(CollectionResult));
            Assert.IsTrue(signature.ReturnType!.Equals(expectedReturnType));
        }

        [TestCase(true, InputRequestLocation.Header)]
        [TestCase(true, InputRequestLocation.Body)]
        [TestCase(false, InputRequestLocation.Header)]
        [TestCase(false, InputRequestLocation.Body)]
        public void ListMethodWithEnumParameter(bool isExtensible, InputRequestLocation location)
        {
            var enumType = InputFactory.StringEnum("color", [("red", "red")], isExtensible: isExtensible);
            IReadOnlyList<InputParameter> parameters =
            [
                InputFactory.Parameter(
                    "color",
                    enumType,
                    location: location,
                    isRequired: true)
            ];
            var pagingMetadata = InputFactory.PagingMetadata(
                ["items"],
                null,
                null);
            var inputModel = InputFactory.Model("cat", properties:
            [
                InputFactory.Property("color", InputPrimitiveType.String, isRequired: true),
            ]);

            var response = InputFactory.OperationResponse(
                [200],
                InputFactory.Model(
                    "page",
                    properties: [InputFactory.Property("cats", InputFactory.Array(inputModel))]));
            var operation = InputFactory.Operation("getCats", responses: [response], parameters: parameters);
            var inputServiceMethod = InputFactory.PagingServiceMethod("Test", operation, pagingMetadata: pagingMetadata, parameters: parameters);
            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);

            MockHelpers.LoadMockGenerator(inputModels: () => [inputModel], clients: () => [inputClient]);
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            Assert.IsNotNull(client);

            // there should be a CollectionResultDefinition
            var collectionResultDefinition = ScmCodeModelGenerator.Instance.OutputLibrary.TypeProviders.FirstOrDefault(
               t => t is CollectionResultDefinition);
            Assert.IsNotNull(collectionResultDefinition);

            var methodCollection = new ScmMethodProviderCollection(inputClient.Methods.First(), client!);
            Assert.IsNotNull(methodCollection);
            Assert.AreEqual(4, methodCollection.Count);

            var convenienceMethod = methodCollection.FirstOrDefault(
                m => !m.Signature.Parameters.Any(p => p.Name == "options") && m.Signature.Name == "GetCats");
            Assert.IsNotNull(convenienceMethod);

            var signature = convenienceMethod!.Signature;
            var expectedReturnType = new CSharpType(typeof(CollectionResult));
            Assert.IsTrue(signature.ReturnType!.Equals(expectedReturnType));

            var colorParameter = signature.Parameters.FirstOrDefault(p => p.Name == "color");
            Assert.IsNotNull(colorParameter);
            var expectedType = ScmCodeModelGenerator.Instance.TypeFactory.CreateEnum(enumType);
            Assert.IsTrue(expectedType!.Type.Equals(colorParameter!.Type));

            if (location == InputRequestLocation.Header)
            {
                if (isExtensible)
                {
                    StringAssert.Contains("color.ToString()", convenienceMethod.BodyStatements!.ToDisplayString());
                }
                else
                {
                    StringAssert.Contains("color.ToSerialString()",
                        convenienceMethod.BodyStatements!.ToDisplayString());
                }
            }
            else
            {
                if (isExtensible)
                {
                    StringAssert.Contains("BinaryData.FromObjectAsJson(color.ToString())", convenienceMethod.BodyStatements!.ToDisplayString());
                }
                else
                {
                    StringAssert.Contains("BinaryData.FromObjectAsJson(color.ToSerialString())",
                        convenienceMethod.BodyStatements!.ToDisplayString());
                }
            }
        }

        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void RequestOptionsOptionality(bool inBody, bool hasOptionalParameter, bool shouldBeOptional)
        {
            MockHelpers.LoadMockGenerator();
            List<InputParameter> parameters =
            [
                InputFactory.Parameter(
                    "message",
                    InputPrimitiveType.Boolean,
                    isRequired: !hasOptionalParameter,
                    location: inBody ? InputRequestLocation.Body : InputRequestLocation.Query)
            ];
            var inputOperation = InputFactory.Operation(
                "TestOperation",
                parameters: parameters);
            var inputServiceMethod = InputFactory.BasicServiceMethod("Test", inputOperation, parameters: parameters);
            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            var protocolMethod = methodCollection.FirstOrDefault(
                m => m.Signature.Parameters.Any(p => p.Name == "options") && m.Signature.Name == "TestOperation");
            Assert.IsNotNull(protocolMethod);
            Assert.AreEqual(inputServiceMethod, protocolMethod!.ServiceMethod);

            var optionsParameter = protocolMethod!.Signature.Parameters.Single(p => p.Name == "options");
            Assert.AreEqual(shouldBeOptional, optionsParameter.DefaultValue != null);

            if (!shouldBeOptional)
            {
                Assert.IsTrue(protocolMethod.Signature.Parameters.All(p => p.DefaultValue == null));
            }
        }

        [Test]
        public void RequestOptionsIsOptionalWhenNoConvenience()
        {
            MockHelpers.LoadMockGenerator();
            var inputOperation = InputFactory.Operation(
                "TestOperation",
                generateConvenienceMethod: false);
            var inputServiceMethod = InputFactory.BasicServiceMethod("Test", inputOperation);
            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            var protocolMethod = methodCollection.FirstOrDefault(
                m => m.Signature.Parameters.Any(p => p.Name == "options") && m.Signature.Name == "TestOperation");
            Assert.IsNotNull(protocolMethod);
            Assert.AreEqual(inputServiceMethod, protocolMethod!.ServiceMethod);

            var optionsParameter = protocolMethod!.Signature.Parameters.Single(p => p.Name == "options");
            Assert.IsNotNull(optionsParameter.DefaultValue);
        }

        [Test]
        public void OperationWithOptionalEnum()
        {
            MockHelpers.LoadMockGenerator();
            ScmMethodProvider? convenienceMethod = SetupOptionalEnumTest(false);

            var statements = convenienceMethod!.BodyStatements!.ToDisplayString();
            StringAssert.Contains("choice?.ToSerialString()", statements);
        }

        [Test]
        public void OperationWithOptionalExtensibleEnum()
        {
            MockHelpers.LoadMockGenerator();
            ScmMethodProvider? convenienceMethod = SetupOptionalEnumTest(true);

            var statements = convenienceMethod!.BodyStatements!.ToDisplayString();
            StringAssert.Contains("choice?.ToString()", statements);
        }

        [Test]
        public void OperationWithOptionalIntEnum()
        {
            MockHelpers.LoadMockGenerator();
            ScmMethodProvider? convenienceMethod = SetupOptionalEnumTest(false, true);

            var statements = convenienceMethod!.BodyStatements!.ToDisplayString();
            StringAssert.Contains("(int)choice", statements);
        }

        [Test]
        public void OperationWithOptionalExtensibleIntEnum()
        {
            MockHelpers.LoadMockGenerator();
            ScmMethodProvider? convenienceMethod = SetupOptionalEnumTest(true, true);

            var statements = convenienceMethod!.BodyStatements!.ToDisplayString();
            StringAssert.Contains("choice?.ToSerialInt32()", statements);
        }

        private static ScmMethodProvider? SetupOptionalEnumTest(bool isExtensible, bool useInt = false)
        {
            List<InputParameter> parameters =
            [
                InputFactory.Parameter(
                    "choice",
                    useInt
                        ? InputFactory.Int32Enum("TestEnum", [("Value1", 1), ("Value2", 2)], isExtensible: isExtensible)
                        : InputFactory.StringEnum("TestEnum", [("Value1", "value1"), ("Value2", "value2")], isExtensible: isExtensible),
                    isRequired: false,
                    location: InputRequestLocation.Query)
            ];
            var inputOperation = InputFactory.Operation(
                "TestOperation",
                parameters: parameters);
            var inputServiceMethod = InputFactory.BasicServiceMethod("Test", inputOperation, parameters: parameters);
            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);
            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            var convenienceMethod = methodCollection.FirstOrDefault(
                m => m.Signature.Parameters.All(p => p.Name != "options") && m.Signature.Name == "TestOperation");
            return convenienceMethod;
        }

        private static IEnumerable<TestCaseData> RequestBodyTypesSource()
        {
            yield return new TestCaseData(
                InputFactory.Array(
                    InputFactory.Model("cat", properties:
                    [
                        InputFactory.Property("color", InputPrimitiveType.String, isRequired: true),
                    ])));

            yield return new TestCaseData(
                InputFactory.Dictionary(
                    InputFactory.Model("cat", properties:
                    [
                        InputFactory.Property("color", InputPrimitiveType.String, isRequired: true),
                    ])));

            yield return new TestCaseData(
                    InputFactory.Model("cat", properties:
                    [
                        InputFactory.Property("color", InputPrimitiveType.String, isRequired: true),
                    ]));
        }

        [TestCaseSource(nameof(RequestBodyTypesSource))]
        public void RequestBodyConstructedUsingBinaryContentHelpers(InputType inputType)
        {
            MockHelpers.LoadMockGenerator();
            var parameter = InputFactory.Parameter("message", inputType, isRequired: true);

            var inputOperation = InputFactory.Operation(
                "TestOperation",
                parameters: [parameter]);

            var inputServiceMethod = InputFactory.BasicServiceMethod("TestOperation", inputOperation,
                parameters: [parameter]);

            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);

            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);

            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            var convenienceMethod = methodCollection.FirstOrDefault(
                m => m.Signature.Parameters.Any(p => p.Name == "message") && m.Signature.Name == "TestOperation");
            Assert.IsNotNull(convenienceMethod);

            if (inputType is InputArrayType)
            {
                Assert.IsTrue(convenienceMethod!.BodyStatements!.ToDisplayString()
                    .Contains("using global::System.ClientModel.BinaryContent content = global::Sample.BinaryContentHelper.FromEnumerable(message);"));
            }
            else if (inputType is InputDictionaryType)
            {
                Assert.IsTrue(convenienceMethod!.BodyStatements!.ToDisplayString()
                    .Contains("using global::System.ClientModel.BinaryContent content = global::Sample.BinaryContentHelper.FromDictionary(message);"));
            }
            else
            {
                Assert.IsFalse(convenienceMethod!.BodyStatements!.ToDisplayString().Contains("BinaryContentHelper"));
            }
        }

        [Test]
        public void RequestBodyConstructedUsingReadOnlyMemoryBinaryContentHelpers()
        {
            MockHelpers.LoadMockGenerator(createCSharpTypeCore: _ => new CSharpType(typeof(ReadOnlyMemory<int>)));
            var inputType = InputFactory.Array(InputPrimitiveType.Int32);

            var parameter = InputFactory.Parameter("data", inputType, isRequired: true);

            var inputOperation = InputFactory.Operation(
                "TestOperation",
                parameters: [parameter]);

            var inputServiceMethod = InputFactory.BasicServiceMethod("TestOperation", inputOperation,
                parameters: [parameter]);

            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);

            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            var convenienceMethod = methodCollection.FirstOrDefault(
                m => m.Signature.Parameters.Any(p => p.Name == "data") && m.Signature.Name == "TestOperation");
            Assert.IsNotNull(convenienceMethod);

            Assert.IsTrue(convenienceMethod!.BodyStatements!.ToDisplayString()
                .Contains("using global::System.ClientModel.BinaryContent content = global::Sample.BinaryContentHelper.FromEnumerable(data.Span);"));
        }

        [Test]
        public void CanRemoveParameterFromMethods()
        {
            var parameter1 = InputFactory.Parameter("toRemove", InputPrimitiveType.String, isRequired: true, location: InputRequestLocation.Header);
            var parameter2 = InputFactory.Parameter("data", InputPrimitiveType.String, isRequired: true, location: InputRequestLocation.Header);

            var inputOperation = InputFactory.Operation(
                "TestOperation",
                parameters: [parameter1, parameter2]);

            var inputServiceMethod = InputFactory.BasicServiceMethod("TestOperation", inputOperation,
                parameters: [parameter1, parameter2]);

            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);
            MockHelpers.LoadMockGenerator(createParameterCore: parameter =>
            {
                if (parameter.Name == "toRemove")
                {
                    return null;
                }
                return new ParameterProvider(parameter);
            });
            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);

            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            foreach (var method in methodCollection)
            {
                // Ensure that the parameter "toRemove" is not present in the method signature
                Assert.IsFalse(method.Signature.Parameters.Any(p => p.Name == "toRemove"));
            }

            var restClient = client!.RestClient;
            foreach (var method in restClient.Methods)
            {
                // Ensure that the parameter "toRemove" is not present in the rest client method signature
                Assert.IsFalse(method.Signature.Parameters.Any(p => p.Name == "toRemove"));
            }
        }

        [TestCaseSource(nameof(RequestBodyTypesSource))]
        public void RequestBodyConstructedRespectingRequestContentApi(InputType inputType)
        {
            MockHelpers.LoadMockGenerator(requestContentApi: TestRequestContentApi.Instance);
            var parameter = InputFactory.Parameter("message", inputType, isRequired: true);

            var inputOperation = InputFactory.Operation(
                "TestOperation",
                parameters: [parameter]);

            var inputServiceMethod = InputFactory.BasicServiceMethod("TestOperation", inputOperation,
                parameters: [parameter]);

            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);

            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);

            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            var convenienceMethod = methodCollection.FirstOrDefault(
                m => m.Signature.Parameters.Any(p => p.Name == "message") && m.Signature.Name == "TestOperation");
            Assert.IsNotNull(convenienceMethod);

            if (inputType is InputArrayType)
            {
                Assert.IsTrue(convenienceMethod!.BodyStatements!.ToDisplayString()
                    .Contains("using string content = global::Sample.BinaryContentHelper.FromEnumerable(message);"));
            }
            else if (inputType is InputDictionaryType)
            {
                Assert.IsTrue(convenienceMethod!.BodyStatements!.ToDisplayString()
                    .Contains("using string content = global::Sample.BinaryContentHelper.FromDictionary(message);"));
            }
            else
            {
                Assert.IsFalse(convenienceMethod!.BodyStatements!.ToDisplayString().Contains("BinaryContentHelper"));
            }
        }

        [Test]
        public void ListMethodIsRenamedToGet()
        {
            MockHelpers.LoadMockGenerator(requestContentApi: TestRequestContentApi.Instance);

            var inputOperation = InputFactory.Operation(
                "ListCats");

            var inputServiceMethod = InputFactory.BasicServiceMethod("ListCats", inputOperation);

            var inputClient = InputFactory.Client("TestClient", methods: [inputServiceMethod]);

            var client = ScmCodeModelGenerator.Instance.TypeFactory.CreateClient(inputClient);

            var methodCollection = new ScmMethodProviderCollection(inputServiceMethod, client!);
            Assert.IsNotNull(methodCollection);
            foreach (var method in methodCollection)
            {
                if (method.Signature.Modifiers.HasFlag(MethodSignatureModifiers.Async))
                {
                    Assert.AreEqual("GetCatsAsync", method.Signature.Name);
                }
                else
                {
                    Assert.AreEqual("GetCats", method.Signature.Name);
                }
            }
        }

        public static IEnumerable<TestCaseData> DefaultCSharpMethodCollectionTestCases
        {
            get
            {
                yield return new TestCaseData(InputFactory.BasicServiceMethod(
                    "CreateMessage",
                    InputFactory.Operation(
                        "CreateMessage",
                        parameters:
                        [
                            InputFactory.Parameter(
                                "message",
                                InputPrimitiveType.Boolean,
                                isRequired: true)
                        ]),
                    parameters:
                    [
                        InputFactory.Parameter(
                            "message",
                            InputPrimitiveType.Boolean,
                            isRequired: true)
                    ]));

                // method with spread parameter
                yield return new TestCaseData(InputFactory.BasicServiceMethod(
                    "CreateMessage",
                    InputFactory.Operation(
                        "CreateMessage",
                        parameters:
                        [
                            InputFactory.Parameter(
                                "spread",
                                _spreadModel,
                                location: InputRequestLocation.Body,
                                isRequired: true,
                                kind: InputParameterKind.Spread),
                            InputFactory.Parameter(
                                "p1",
                                InputPrimitiveType.Boolean,
                                location: InputRequestLocation.Path,
                                isRequired: true,
                                kind: InputParameterKind.Method)
                        ]),
                    parameters:
                    [
                        InputFactory.Parameter("p2", InputPrimitiveType.String, isRequired: true, kind: InputParameterKind.Spread),
                        InputFactory.Parameter(
                                "p1",
                                InputPrimitiveType.Boolean,
                                location: InputRequestLocation.Path,
                                isRequired: true,
                                kind: InputParameterKind.Method)
                    ]));
            }
        }
    }
}

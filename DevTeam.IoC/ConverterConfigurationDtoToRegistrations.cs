namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConverterConfigurationDtoToRegistrations: IConverter<IConfigurationDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterConfigurationDtoToRegistrations.Context>
    {
        [NotNull] private readonly IConverter<ITagDto, object, TypeResolverContext> _converterTagDtoToObject;
        [NotNull] private readonly IConverter<IRegisterDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterRegisterDtoToRegistations.Context> _converterRegisterDtoToRegistationResult;

        public ConverterConfigurationDtoToRegistrations(
            [NotNull] IConverter<ITagDto, object, TypeResolverContext> converterTagDtoToObject,
            [NotNull] IConverter<IRegisterDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterRegisterDtoToRegistations.Context> converterRegisterDtoToRegistationResult)
        {
            if (converterTagDtoToObject == null) throw new ArgumentNullException(nameof(converterTagDtoToObject));
            if (converterRegisterDtoToRegistationResult == null) throw new ArgumentNullException(nameof(converterRegisterDtoToRegistationResult));
            _converterTagDtoToObject = converterTagDtoToObject;
            _converterRegisterDtoToRegistationResult = converterRegisterDtoToRegistationResult;
        }

        public bool TryConvert(IConfigurationDto configurationElements, out IEnumerable<IRegistrationResult<IContainer>> value, Context context)
        {
            value = Convert(configurationElements, context);
            return true;
        }

        private IEnumerable<IRegistrationResult<IContainer>> Convert(IEnumerable<IConfigurationStatementDto> configurationElements, Context context)
        {
            var references = new List<Assembly>(context.TypeResolverContext.References);
            var usings = new List<string>(context.TypeResolverContext.Usings);
            foreach (var configurationStatement in configurationElements)
            {
                var referenceDto = configurationStatement as IReferenceDto;
                if (referenceDto != null)
                {
                    references.Add(Assembly.Load(new AssemblyName(referenceDto.Reference)));
                    continue;
                }

                var usingDto = configurationStatement as IUsingDto;
                if (usingDto != null)
                {
                    usings.Add(usingDto.Using);
                    continue;
                }

                var containerDto = configurationStatement as IContainerDto;
                if (containerDto != null)
                {
                    var curTypeResolverContext = new TypeResolverContext(references, usings);
                    object containerTag = null;
                    if (containerDto.Tag != null)
                    {
                        if (!_converterTagDtoToObject.TryConvert(containerDto.Tag, out containerTag, curTypeResolverContext))
                        {
                            throw new Exception($"Invalid container tag {containerDto.Tag.Value}");
                        }
                    }

                    //foreach (var registration in Apply(container.CreateChild(containerTag), reflection, curTypeResolverContext, containerDto.Statements))
                    foreach (var registration in Convert(containerDto.Statements, new Context(context.Container.CreateChild(containerTag), new TypeResolverContext(references, usings))))
                    {
                        yield return registration;
                    }

                    continue;
                }

                var registerDto = configurationStatement as IRegisterDto;
                if (registerDto != null)
                {
                    IEnumerable<IRegistrationResult<IContainer>> registrations;
                    if (!_converterRegisterDtoToRegistationResult.TryConvert(registerDto, out registrations, new ConverterRegisterDtoToRegistations.Context(context.Container, new TypeResolverContext(references, usings))))
                    {
                        throw new Exception($"Invalid defenition of {registerDto.AutowiringTypeName}");
                    }

                    foreach (var registration in registrations)
                    {
                        yield return registration;
                    }
                }
            }
        }

        internal sealed class Context
        {
            public Context([NotNull] IContainer container, [NotNull] TypeResolverContext typeResolverContext)
            {
                if (container == null) throw new ArgumentNullException(nameof(container));
                if (typeResolverContext == null) throw new ArgumentNullException(nameof(typeResolverContext));
                Container = container;
                TypeResolverContext = typeResolverContext;
            }

            public IContainer Container { get; }

            public TypeResolverContext TypeResolverContext { get; }
        }
    }
}

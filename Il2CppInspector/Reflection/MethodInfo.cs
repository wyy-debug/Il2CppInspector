﻿/*
    Copyright 2017 Katy Coe - http://www.hearthcode.org - http://www.djkaty.com

    All rights reserved.
*/

using System;
using System.Linq;
using System.Reflection;

namespace Il2CppInspector.Reflection
{
    public class MethodInfo : MethodBase
    {
        // IL2CPP-specific data
        public Il2CppMethodDefinition Definition { get; }
        public int Index { get; }
        public uint VirtualAddress { get; }
        public bool HasBody { get; }

        public override MemberTypes MemberType => MemberTypes.Method;

        // Info about the return parameter
        public ParameterInfo ReturnParameter { get; }

        // Return type of the method
        private readonly Il2CppType returnType;
        public TypeInfo ReturnType => Assembly.Model.GetType(returnType, MemberTypes.TypeInfo);

        // TODO: ReturnTypeCustomAttributes

        public MethodInfo(Il2CppInspector pkg, int methodIndex, TypeInfo declaringType) :
            base(declaringType) {
            Definition = pkg.Metadata.Methods[methodIndex];
            Index = methodIndex;
            if (Definition.methodIndex >= 0) {
                VirtualAddress = pkg.Binary.MethodPointers[Definition.methodIndex];
                HasBody = true;
            }
            Name = pkg.Strings[Definition.nameIndex];

            if ((Definition.flags & Il2CppConstants.METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == Il2CppConstants.METHOD_ATTRIBUTE_PRIVATE)
                Attributes |= MethodAttributes.Private;
            if ((Definition.flags & Il2CppConstants.METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == Il2CppConstants.METHOD_ATTRIBUTE_PUBLIC)
                Attributes |= MethodAttributes.Public;
            if ((Definition.flags & Il2CppConstants.METHOD_ATTRIBUTE_VIRTUAL) != 0)
                Attributes |= MethodAttributes.Virtual;
            if ((Definition.flags & Il2CppConstants.METHOD_ATTRIBUTE_STATIC) != 0)
                Attributes |= MethodAttributes.Static;

            // Add return parameter
            returnType = pkg.TypeUsages[Definition.returnType];
            ReturnParameter = new ParameterInfo(pkg, -1, this);

            // Add arguments
            for (var p = Definition.parameterStart; p < Definition.parameterStart + Definition.parameterCount; p++)
                DeclaredParameters.Add(new ParameterInfo(pkg, p, this));
        }

        public override string ToString() => ReturnType.Name + " " + Name + "(" + string.Join(", ", DeclaredParameters.Select(x => x.ParameterType.Name)) + ")";
    }
}
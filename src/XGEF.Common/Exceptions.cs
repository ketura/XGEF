///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
////    Copyright 2017 Christian 'ketura' McCarty                                  ////
////                                                                               ////
////    Licensed under the Apache License, Version 2.0 (the "License");            ////
////    you may not use this file except in compliance with the License.           ////
////    You may obtain a copy of the License at                                    ////
////                                                                               ////
////                http://www.apache.org/licenses/LICENSE-2.0                     ////
////                                                                               ////
////    Unless required by applicable law or agreed to in writing, software        ////
////    distributed under the License is distributed on an "AS IS" BASIS,          ////
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   ////
////    See the License for the specific language governing permissions and        ////
////    limitations under the License.                                             ////
////                                                                               ////
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGEF
{
	public class CompilationErrorException : Exception
	{
		public List<String> Errors { get; private set; }
		public CompilationErrorException() : base() { }
		public CompilationErrorException(string message) : this(message, new List<string>()) { }
		public CompilationErrorException(string message, IEnumerable<string> errors) : base(message)
		{
			Errors = errors.ToList();
		}
		public CompilationErrorException(string message, Exception inner) : this(message, inner, new List<string>()) { }
		public CompilationErrorException(string message, Exception inner, IEnumerable<string> errors) : base(message, inner)
		{
			Errors = errors.ToList();
		}
	}

	public class PreprocessorErrorException : CompilationErrorException
	{
		public PreprocessorErrorException() : base() { }
		public PreprocessorErrorException(string message) : base(message) { }
		public PreprocessorErrorException(string message, Exception inner) : base(message, inner) { }
	}

	public class InvalidAncestorException : PreprocessorErrorException
	{
		public InvalidAncestorException() : base() { }
		public InvalidAncestorException(string message) : base(message) { }
		public InvalidAncestorException(string message, Exception inner) : base(message, inner) { }
	}

	public class UndeclaredOverwriteException : PreprocessorErrorException
	{
		public UndeclaredOverwriteException() : base() { }
		public UndeclaredOverwriteException(string message) : base(message) { }
		public UndeclaredOverwriteException(string message, Exception inner) : base(message, inner) { }
	}

	public class InvalidAttributeException : PreprocessorErrorException
	{
		public InvalidAttributeException() : base() { }
		public InvalidAttributeException(string message) : base(message) { }
		public InvalidAttributeException(string message, Exception inner) : base(message, inner) { }
	}

	public class ConflictingAttributeException : PreprocessorErrorException
	{
		public ConflictingAttributeException() : base() { }
		public ConflictingAttributeException(string message) : base(message) { }
		public ConflictingAttributeException(string message, Exception inner) : base(message, inner) { }
	}

	public class DuplicateAttributeException : PreprocessorErrorException
	{
		public DuplicateAttributeException() : base() { }
		public DuplicateAttributeException(string message) : base(message) { }
		public DuplicateAttributeException(string message, Exception inner) : base(message, inner) { }
	}

	public class UnsupportedTransformationException : PreprocessorErrorException
	{
		public UnsupportedTransformationException() : base() { }
		public UnsupportedTransformationException(string message) : base(message) { }
		public UnsupportedTransformationException(string message, Exception inner) : base(message, inner) { }
	}



	public class CircularReferenceException : PreprocessorErrorException
	{
		public CircularReferenceException() : base() { }
		public CircularReferenceException(string message) : base(message) { }
		public CircularReferenceException(string message, Exception inner) : base(message, inner) { }
	}
}

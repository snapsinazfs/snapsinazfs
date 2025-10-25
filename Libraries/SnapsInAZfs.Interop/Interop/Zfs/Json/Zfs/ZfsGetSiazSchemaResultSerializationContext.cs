// Copyright $CurrentDate.Year Brandon Thetford
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// See https://opensource.org/license/MIT/

namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSerializable ( typeof( ZfsGetSiazSchemaResult ) )]
[JsonSerializable ( typeof( ZfsGetSiazDatasetSchemaBase ) )]
[JsonSerializable ( typeof( Dictionary<string, ZfsGetSiazDatasetSchemaBase> ) )]
[JsonSourceGenerationOptions (
                               JsonSerializerDefaults.General,
                               AllowOutOfOrderMetadataProperties = true,
                               DefaultBufferSize = 16384,
                               UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                               DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                               RespectNullableAnnotations = true
                             )]
[UsedImplicitly (
                  ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature
                | ImplicitUseKindFlags.Access,
                  ImplicitUseTargetFlags.WithMembers
                | ImplicitUseTargetFlags.WithInheritors,
                  Reason = "Used by the JSON serializer."
                )]
public partial class ZfsGetSiazSchemaResultSerializationContext : JsonSerializerContext
{
}

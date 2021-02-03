﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tomlet.Tests {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class TestResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TestResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Tomlet.Tests.TestResources", typeof(TestResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # fractional
        ///flt1 = +1.0
        ///flt2 = 3.1415
        ///flt3 = -0.01
        ///
        ///# exponent
        ///flt4 = 5e+22
        ///flt5 = 1e06
        ///flt6 = -2E-2
        ///
        ///# both
        ///flt7 = 6.626e-34.
        /// </summary>
        internal static string BasicFloatTestInput {
            get {
                return ResourceManager.GetString("BasicFloatTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to int1 = +99
        ///int2 = 42
        ///int3 = 0
        ///int4 = -17.
        /// </summary>
        internal static string BasicIntegerTestInput {
            get {
                return ResourceManager.GetString("BasicIntegerTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to key = &quot;value&quot;.
        /// </summary>
        internal static string BasicKVPTestInput {
            get {
                return ResourceManager.GetString("BasicKVPTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;&quot; = &quot;blank&quot;     # VALID but discouraged.
        /// </summary>
        internal static string BlankKeysAreAcceptedTestInput {
            get {
                return ResourceManager.GetString("BlankKeysAreAcceptedTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to bool1 = true
        ///bool2 = false.
        /// </summary>
        internal static string BooleanTestInput {
            get {
                return ResourceManager.GetString("BooleanTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # This is a full-line comment
        ///key = &quot;value&quot;  # This is a comment at the end of a line
        ///another = &quot;# This is not a comment&quot;.
        /// </summary>
        internal static string CommentTestInput {
            get {
                return ResourceManager.GetString("CommentTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to str4 = &quot;&quot;&quot;Here are two quotation marks: &quot;&quot;. Simple enough.&quot;&quot;&quot;
        ///str5 = &quot;&quot;&quot;Here are three quotation marks: &quot;&quot;\&quot;.&quot;&quot;&quot;
        ///str6 = &quot;&quot;&quot;Here are fifteen quotation marks: &quot;&quot;\&quot;&quot;&quot;\&quot;&quot;&quot;\&quot;&quot;&quot;\&quot;&quot;&quot;\&quot;.&quot;&quot;&quot;
        ///
        ///# &quot;This,&quot; she said, &quot;is just a pointless statement.&quot;
        ///str7 = &quot;&quot;&quot;&quot;This,&quot; she said, &quot;is just a pointless statement.&quot;&quot;&quot;&quot;.
        /// </summary>
        internal static string DoubleQuotesInMultilineBasicTestInput {
            get {
                return ResourceManager.GetString("DoubleQuotesInMultilineBasicTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to = &quot;no key name&quot;  # INVALID.
        /// </summary>
        internal static string EmptyKeyNameTestInput {
            get {
                return ResourceManager.GetString("EmptyKeyNameTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to str = &quot;I&apos;m a string. \&quot;You can quote me\&quot;. Name\tJos\u00E9\nLocation\tSF.&quot;.
        /// </summary>
        internal static string EscapedDoubleQuotedStringTestInput {
            get {
                return ResourceManager.GetString("EscapedDoubleQuotedStringTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # infinity
        ///sf1 = inf  # positive infinity
        ///sf2 = +inf # positive infinity
        ///sf3 = -inf # negative infinity
        ///
        ///# not a number
        ///sf4 = nan  # actual sNaN/qNaN encoding is implementation-specific
        ///sf5 = +nan # same as `nan`
        ///sf6 = -nan # valid, actual encoding is implementation-specific.
        /// </summary>
        internal static string FloatSpecialsTestInput {
            get {
                return ResourceManager.GetString("FloatSpecialsTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to flt8 = 224_617.445_991_228.
        /// </summary>
        internal static string FloatWithUnderscoresTestInput {
            get {
                return ResourceManager.GetString("FloatWithUnderscoresTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to winpath  = &apos;C:\Users\nodejs\templates&apos;
        ///winpath2 = &apos;\\ServerX\admin$\system32\&apos;
        ///quoted   = &apos;Tom &quot;Dubs&quot; Preston-Werner&apos;
        ///regex    = &apos;&lt;\i\c*\s*&gt;&apos;
        ///empty = &apos;&apos;.
        /// </summary>
        internal static string LiteralStringTestInput {
            get {
                return ResourceManager.GetString("LiteralStringTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ld1 = 1979-05-27.
        /// </summary>
        internal static string LocalDateTestInput {
            get {
                return ResourceManager.GetString("LocalDateTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ldt1 = 1979-05-27T07:32:00
        ///ldt2 = 1979-05-27T00:32:00.999999.
        /// </summary>
        internal static string LocalDateTimeTestInput {
            get {
                return ResourceManager.GetString("LocalDateTimeTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to lt1 = 07:32:00
        ///lt2 = 00:32:00.999999.
        /// </summary>
        internal static string LocalTimeTestInput {
            get {
                return ResourceManager.GetString("LocalTimeTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to regex2 = &apos;&apos;&apos;I [dw]on&apos;t need \d{2} apples&apos;&apos;&apos;
        ///lines  = &apos;&apos;&apos;
        ///The first newline is
        ///trimmed in raw strings.
        ///   All other whitespace
        ///   is preserved.
        ///&apos;&apos;&apos;.
        /// </summary>
        internal static string MultiLineLiteralStringTestInput {
            get {
                return ResourceManager.GetString("MultiLineLiteralStringTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to str1 = &quot;&quot;&quot;
        ///Roses are red
        ///Violets are blue&quot;&quot;&quot;.
        /// </summary>
        internal static string MultiLineSimpleStringTestInput {
            get {
                return ResourceManager.GetString("MultiLineSimpleStringTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to first = &quot;Tom&quot; last = &quot;Preston-Werner&quot; # INVALID.
        /// </summary>
        internal static string MultiplePairsOnOneLineTestInput {
            get {
                return ResourceManager.GetString("MultiplePairsOnOneLineTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to nested_arrays_of_ints = [ [ 1, 2 ], [3, 4, 5] ]
        ///nested_mixed_array = [ [ 1, 2 ], [&quot;a&quot;, &quot;b&quot;, &quot;c&quot;] ].
        /// </summary>
        internal static string NestedArraysTestInput {
            get {
                return ResourceManager.GetString("NestedArraysTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to key = &quot;value&quot;
        ///bare_key = &quot;value&quot;
        ///bare-key = &quot;value&quot;
        ///1234 = &quot;value&quot;.
        /// </summary>
        internal static string NonSimpleKeysTestInput {
            get {
                return ResourceManager.GetString("NonSimpleKeysTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to odt1 = 1979-05-27T07:32:00Z
        ///odt2 = 1979-05-27T00:32:00-07:00
        ///odt3 = 1979-05-27T00:32:00.999999-07:00
        ///odt4 = 1979-05-27 07:32:00Z.
        /// </summary>
        internal static string OffsetDateTimeTestInput {
            get {
                return ResourceManager.GetString("OffsetDateTimeTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to integers = [ 1, 2, 3 ]
        ///colors = [ &quot;red&quot;, &quot;yellow&quot;, &quot;green&quot; ]
        ///string_array = [ &quot;all&quot;, &apos;strings&apos;, &quot;&quot;&quot;are the same&quot;&quot;&quot;, &apos;&apos;&apos;type&apos;&apos;&apos; ].
        /// </summary>
        internal static string PrimitiveArraysTestInput {
            get {
                return ResourceManager.GetString("PrimitiveArraysTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;127.0.0.1&quot; = &quot;value&quot;
        ///&quot;character encoding&quot; = &quot;value&quot;
        ///&quot;ʎǝʞ&quot; = &quot;value&quot;
        ///&apos;key2&apos; = &quot;value&quot;
        ///&apos;quoted &quot;value&quot;&apos; = &quot;value&quot;.
        /// </summary>
        internal static string QuotedKeysTestInput {
            get {
                return ResourceManager.GetString("QuotedKeysTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to quot15 = &apos;&apos;&apos;Here are fifteen quotation marks: &quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&quot;&apos;&apos;&apos;
        ///
        ///# apos15 = &apos;&apos;&apos;Here are fifteen apostrophes: &apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;  # INVALID
        ///apos15 = &quot;Here are fifteen apostrophes: &apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&apos;&quot;
        ///
        ///# &apos;That,&apos; she said, &apos;is still pointless.&apos;
        ///str = &apos;&apos;&apos;&apos;That,&apos; she said, &apos;is still pointless.&apos;&apos;&apos;&apos;.
        /// </summary>
        internal static string SingleQuotesInMultilineLiteralTestInput {
            get {
                return ResourceManager.GetString("SingleQuotesInMultilineLiteralTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to int5 = 1_000
        ///int6 = 5_349_221
        ///int7 = 53_49_221  # Indian number system grouping
        ///int8 = 1_2_3_4_5  # VALID but discouraged.
        /// </summary>
        internal static string UnderscoresInIntegersTestInput {
            get {
                return ResourceManager.GetString("UnderscoresInIntegersTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to key = # INVALID .
        /// </summary>
        internal static string UnspecifiedValueTestInput {
            get {
                return ResourceManager.GetString("UnspecifiedValueTestInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to str1 = &quot;The quick brown fox jumps over the lazy dog.&quot;
        ///
        ///str2 = &quot;&quot;&quot;
        ///The quick brown \
        ///
        ///
        ///  fox jumps over \
        ///    the lazy dog.&quot;&quot;&quot;
        ///
        ///str3 = &quot;&quot;&quot;\
        ///       The quick brown \
        ///       fox jumps over \
        ///       the lazy dog.\
        ///       &quot;&quot;&quot;.
        /// </summary>
        internal static string WhitespaceRemovalTestInput {
            get {
                return ResourceManager.GetString("WhitespaceRemovalTestInput", resourceCulture);
            }
        }
    }
}

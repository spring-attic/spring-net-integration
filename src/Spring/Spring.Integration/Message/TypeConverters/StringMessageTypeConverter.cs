
using System;
using System.ComponentModel;
using System.Globalization;

namespace Spring.Integration.Message.TypeConverters {
    /// <summary>
    /// TypeConverter class for <see cref="StringMessage"/>
    /// </summary>
    public class StringMessageTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {

            if(sourceType == typeof(string)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if(value is string) {
                return new StringMessage((string)value);
            }
            return base.ConvertFrom(context, culture, value);
        }
        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if(destinationType == typeof(string)) {
                return ((StringMessage) value).Payload;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
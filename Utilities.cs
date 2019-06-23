using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor
{
    public static class Utilities
    {
        public static string GetRubyRequirements()
        {
            return @"
load_assembly 'IronRuby.Libraries', 'IronRuby.StandardLibrary.Zlib'

class Species
  Moveset = Struct.new(:level, :tms, :tutor, :evolution, :egg)
end
Stats = Struct.new(:hp, :attack, :defense, :spatk, :spdef, :speed)

module MKD
  class Tileset; end
end

def validate_mkd(data, filename = nil)
  errmsg = nil
  if !data.is_a?(Hash)
    errmsg = ""File content is not a Hash.""
  elsif !data[:type]
    errmsg = ""File content does not contain a type header key.""
  elsif !data[:data]
    errmsg = ""File content does not contain a data header key.""
  end
  if errmsg
    raise ""Invalid MKD file#{filename ? "" - #{filename}"" : "".""}\n\n"" + errmsg
  end
end

module FileUtils
  module_function

  def load_data(filename)
    data = File.open(filename, 'rb') do |f|
      next Marshal.load(Zlib::Inflate.inflate(Marshal.load(f)).reverse)
    end
    validate_mkd(data, filename)
    return data[:data]
  end

  def save_data(filename, type, data)
    f = File.new(filename, 'wb')
    Marshal.dump(Zlib::Deflate.deflate(Marshal.dump({type: type, data: data}).reverse), f)
    f.close
    return nil
  end
end

";
        }
    }
}

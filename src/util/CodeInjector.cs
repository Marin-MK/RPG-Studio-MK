using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public static class CodeInjector
{
	public static string FirstScript => @"require 'socket'

def pbConnectToEditor
  $Editor = nil
  $EditorReady = false
  t = Thread.new do
    begin
      $Editor = server = TCPSocket.open(""localhost"", 59995)
      puts ""Server connected.""
      begin
        while true
          data = server.gets.chomp
          puts ""Server :: #{data}""
          if data == ""ping""
            server.puts ""keep-alive""
            server.flush
          elsif data == ""close""
            break
          elsif data == ""ready""
            $EditorReady = true
          else
            # Handle arbitrary message
          end
        end
      ensure
        server.close
        puts ""Server disconnected.""
      end
    rescue
      # Likely no server open at port 59995,
      # or the server quit when the game is still running.
    end
  end
  t.abort_on_exception = true
end

def pbMessageEditor(txt)
  if $Editor && $EditorReady
    $Editor.puts(txt)
    $Editor.flush
  end
end

def pbDisconnectFromEditor
  $Editor.close if $Editor
  $Editor = nil
  $EditorReady = false
end

pbConnectToEditor""";

    public static string MainScript => @"";
}

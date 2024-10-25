local base_path = string.match(get_current_path(), '^(.-)[^/\\]*$')
package.path = string.format("%s;%s?.lua", package.path, base_path)

require("frames_hi_res")

enable_auto_carriage_return(false)

play(get_current_path().."bad_apple.wav")
clear()

for i = 0, #frames do
   home()
   print(frames[i])
   wait(23)
end
local base_path = string.match(get_current_path(), '^(.-)[^/\\]*$')
package.path = string.format("%s;%s?.lua", package.path, base_path)

require("frames_low_res")

function play_decoder(aFrameBuffer)
	clear()
	count = 0
	
	for i, aFrameRow in ipairs(aFrameBuffer) do
		line = ""
		
		for j, packedFrame in ipairs(aFrameRow) do
			for bc = 1, 8, 1 do
				bit = packedFrame & 1
				packedFrame = packedFrame >> 1
				
				if(bit == 1) then
					line = line.."x"
				else
					line = line.." "
				end
			end
		end
		
		if(count < 30)  then
			count = count + 1
			print(line)
		else
			count = 0
			home()
			wait(23)
		end
	end
end


-- Main program
--play(get_current_path().."bad_apple.wav")
play_decoder(frames)
clear();
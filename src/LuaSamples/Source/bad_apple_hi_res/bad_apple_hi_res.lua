--[[
  MiniDOS
  Copyright (C) 2024  Lara H. Ferreira and others.
  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
 
  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
]]--

local __FREQ_SLEEP = 23

local base_path = string.match(get_current_path(), '^(.-)[^/\\]*$')
package.path = string.format("%s;%s?.data;%s?.lua", package.path, base_path, base_path)

require("frames_hi_res")


function play_decoder(aFrameBuffer)
	clear()
	
	for i, aFrameRow in ipairs(aFrameBuffer) do	
		for j, frame_buffer in ipairs(aFrameRow) do
			print(frame_buffer)
		end

		home()
		wait(__FREQ_SLEEP)
	end
end


-- Main program

play(get_current_path().."bad_apple.wav")
play_decoder(frames)
clear();
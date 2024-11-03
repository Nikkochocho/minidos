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

local __MAX_ROWS   = 30
local __MAX_COLS   = 79
local __FREQ_SLEEP = 23
local __COL_RATIO  = 1
local __ROW_RATIO  = 1

local base_path = string.match(get_current_path(), '^(.-)[^/\\]*$')
package.path = string.format("%s;%s?.lua", package.path, base_path)

require("frames_compressed")

function play_decoder(aFrameBuffer)
	local lcount  = 0
	local ccount  = 0
	local rowstep = 0
	local colstep = 0
	local frame_buffer

	clear()
	
	for i, aFrameRow in ipairs(aFrameBuffer) do
		frame_buffer = ""
		ccount = 0

		if(rowstep == __ROW_RATIO)  then
			rowstep = 0
		else
			rowstep = rowstep + 1
			goto next_row
		end

		for j, packedFrame in ipairs(aFrameRow) do
			for bc = 1, 8, 1 do

				if(colstep == __COL_RATIO)  then
					colstep = 0
				else
					colstep = colstep + 1
					goto next_col
				end

				bit = packedFrame & 1
				packedFrame = packedFrame >> 1
				
				if(bit == 1) then
					frame_buffer = frame_buffer.."x"
				else
					frame_buffer = frame_buffer.." "
				end

				::next_col::

				ccount = ccount + 1

				if(ccount > __MAX_COLS)  then
					break;
				end;
			end

			if(ccount > __MAX_COLS)  then
				break;
			end;
		end

		print(frame_buffer)

	    ::next_row::

		lcount = lcount + 1
		
		if(lcount > __MAX_ROWS)  then
			home()
			wait(__FREQ_SLEEP)
			lcount = 0
		end
	end
end


-- Main program

--play(get_current_path().."bad_apple.wav")
enable_screen_compression(true)
play_decoder(frames)
clear();
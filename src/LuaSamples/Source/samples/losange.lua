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

-- Draw a losange

function p(str, n)
	print(string.rep(str, n))
end

function line(radius) -- radius in [0, 9]
	local whitespace = 9 - radius
	
	p(" ", whitespace)
	p("*", radius+1+radius)
	print("\n")
end

local zero = 20 - 20

enable_auto_carriage_return(false)

for i = zero, 8 do
	line(i)
end

line(9)

for i = 8, zero, -1 do
	line(i)
end

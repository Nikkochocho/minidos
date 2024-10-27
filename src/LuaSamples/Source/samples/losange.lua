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

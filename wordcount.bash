find . -name '*.cs' ! -path '*/obj/*' ! -path '*/VonEX/*' ! -path '*/UITextbox/*' ! -path '*/Presenter/*' ! -path '*/LSBgenerator/*' ! -path '*/HyperdeckControl/*' | sed 's/.*/"&"/' | xargs  wc -l

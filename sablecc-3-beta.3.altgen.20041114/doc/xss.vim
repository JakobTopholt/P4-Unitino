" Vim syntax file
" Language: xss
" Maintainer: Indrek Mandre <indrek@mare.ee>
" Last Change:  2003 Dec 07
" Remark: SableCC stylehsheet syntax

" To install, put this file in ~/.vim/syntax/
" and add the lines
"   autocmd BufEnter *.vim set syntax=xss
"   au Syntax xss so $HOME/.vim/syntax/xss.vim
" to your ~/.vimrc file

if version < 600
  syntax clear
elseif exists("b:current_syntax")
  finish
endif

syn case ignore

syn keyword command foreach in template call param set include if else end output choose when otherwise sep contained
syn match operators /[+()]/ contained

syn match inline_tag_comment /\/\/.*/ contained

syn match data_errors /@/
syn match data_errors /\$/

syn region data_errors start=/<xsl:/ end=/>/
syn region data_errors start=/<\/xsl:/ end=/>/

syn match double_at /@@/
syn match double_dollar /\$\$/

syn region inline_xpath start=/${/ end=/}/ skip=/}}/

syn match cmd_errors /{/ contained
syn match cmd_errors /}/ contained
syn region xpath start=/{/ end=/}/ skip=/}}/ contained

syn match inline_identifier /@[a-z][a-z_0-9]*/
syn match inline_identifier /@{[a-z][a-z_0-9]*}/
syn match inline_identifier /\$[a-z][a-z_0-9]*/
syn region cmd_tag start=/\[-/ end=/-\]/ contains=command,operators,inline_identifier,xpath,literal_string,cmd_errors,inline_tag_comment

syn region cmd_tag start=/^\$ / end=/$/ contains=command,operators,inline_identifier,xpath,literal_string,cmd_errors,inline_tag_comment
syn region cmd_tag start=/^\$$/ end=/$/
syn region tag_comment start=/\[-!/ end=/!-\]/
syn region literal_string start=/"/ skip=/""/ end=/"/ contained
syn region literal_string start=/'/ skip=/''/ end=/'/ contained

syn match tag_escape /\[--/

highlight link command Statement
highlight link operators Operator
highlight link inline_identifier Identifier
highlight link inline_xpath Structure
highlight link xpath Structure
highlight link cmd_tag Tag
highlight link tag_comment Comment
highlight link literal_string String
highlight link cmd_errors Error
highlight link data_errors Error
highlight link inline_tag_comment Comment
highlight link xpath_errors Error

let b:current_syntax = "xss"


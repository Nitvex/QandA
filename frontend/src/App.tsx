/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import React from 'react';
import { fontFamily, fontSize, gray2 } from './Styles';
import './App.css';
import { Header } from './Header';
import { HomePage } from './HomePage';

function App() {
  return (
    <div
      css={css`
        font-family: ${fontFamily};
        font-size: ${fontSize};
        color: ${gray2};
      `}
    >
      <Header />
      <HomePage />
    </div>
  );
}

export default App;

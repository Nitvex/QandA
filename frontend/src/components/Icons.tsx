/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import React from 'react';
import user from '../zondicons/user.svg';
export const UserIcon = () => (
  <img
    css={css`
      width: 12px;
      opacity: 0.6;
    `}
    src={user}
    alt="User"
    width="12px"
  />
);
